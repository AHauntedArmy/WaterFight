using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using PhotonHashTable = ExitGames.Client.Photon.Hashtable;
using Utilla;
using Photon.Pun.UtilityScripts;

namespace WaterFight.GameMode
{
    internal class WaterFightGameMode : GorillaGameManager, IPunObservable
    {
        private const float liveDamage = 0.25f;
        private const float deadDamage = 0.15f;
        public const string GameModeKey = "WFGM";
        private List<int> playersMissingMod = new List<int>();

        // float is the percentage of health gone
        private Dictionary<int, float> playerHealth = new Dictionary<int, float>();

        public bool ModRunning { get => playersMissingMod.Count < 1; }

        private float speedLimitDelta;
        private float jumpMultiplyerDelta;

        private bool roundOver = false;
        private float SlipModifier = 0.17f;
        private float roundEndCooldown = 5f;

        public override string GameMode() => "WATERFIGHT";

        public override void Awake()
        {
            // Debug.Log("Water Fight GameMode Awake");
            // Debug.Log("this gameobject enabled " + this.gameObject.activeSelf);
            base.Awake();

            SlipModifier -= GorillaLocomotion.Player.Instance.defaultSlideFactor;
            fastJumpLimit = 8.5f;
            slowJumpLimit = 6.5f;
            fastJumpMultiplier = 1.3f;
            slowJumpMultiplier = 1.1f;
            speedLimitDelta = fastJumpLimit - slowJumpLimit;
            jumpMultiplyerDelta = fastJumpMultiplier - slowJumpMultiplier;

            foreach (var player in PhotonNetwork.PlayerList) {
                if (!player.CustomProperties.ContainsKey(GameModeKey))
                {
                    playersMissingMod.Add(player.ActorNumber);
                
                } else {
                    // Debug.Log($"player {player.NickName} has the mod installed");
                }

                if (!playerHealth.ContainsKey(player.ActorNumber)) {
                    playerHealth.Add(player.ActorNumber, 0f);
                }
            }

            var view = this.photonView;
            if (view == null) {
                Debug.LogError("game manager is missing a photonview");
            
            } else if (photonView.ObservedComponents.Contains(this)) {
               // Debug.Log("game manager is an observed component");
            
            } else {
                // Debug.Log("add game manager to observed components");
                view.ObservedComponents.Add(this);
            }
        }

        public void Start()
        {
            // Debug.Log("Game manager start");

            var view = this.photonView;
            if (view == null) {
                Debug.LogError("game manager is missing a photonview");

            } else if (photonView.ObservedComponents.Contains(this)) {
                // Debug.Log("game manager is an observed component");

            } else {
                // Debug.LogError("game manager is not an observed component");
            }
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            // Debug.Log("water fight game mode manager serialize view");
            if (!ModRunning) {
                return;
            }

            // sending information
            if (stream.IsWriting) {
                var playerHealthEntries = playerHealth.ToArray();
                int entryCount = playerHealthEntries.Count();

                for (int i = 0; i < WaterFightPlugin.MaxPlayers; i++) {

                    if (i < entryCount) {
                        stream.SendNext(playerHealthEntries[i].Key);
                        stream.SendNext(playerHealthEntries[i].Value);
                    
                    } else {
                        stream.SendNext(null);
                        stream.SendNext(null);
                    }
                }
                return;
            }

            // recieving information
            for (int i = 0; i < WaterFightPlugin.MaxPlayers; i++) {
                object key = stream.ReceiveNext();
                object value = stream.ReceiveNext();

                if(key is int actorNumber) {
                    if(!playerHealth.ContainsKey(actorNumber)) {
                        playerHealth.Add(actorNumber, (float)value);
                    
                    } else {
                        playerHealth[actorNumber] = (float)value;
                    }
                }
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);

            if (!newPlayer.CustomProperties.ContainsKey(GameModeKey)) {
                playersMissingMod.Add(newPlayer.ActorNumber);
                Debug.Log("player is missing key");
            }

            if (!playerHealth.ContainsKey(newPlayer.ActorNumber)) {
                if (playerHealth.Count > WaterFightPlugin.MaxPlayers) {
                    Debug.LogWarning("too many players??");

                } else {
                    playerHealth.Add(newPlayer.ActorNumber, 0f);
                }
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            // Debug.Log("on player left room");
            base.OnPlayerLeftRoom(otherPlayer);

            try {
                if (playersMissingMod.Contains(otherPlayer.ActorNumber)) {
                    playersMissingMod.Remove(otherPlayer.ActorNumber);
                }

                if (playerHealth.ContainsKey(otherPlayer.ActorNumber)) {
                    playerHealth.Remove(otherPlayer.ActorNumber);
                }

                if(photonView.IsMine) {
                    CheckRoundEnd();
                }
            } catch { }
        }


        public override void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashTable changedProps)
        {
            base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

            if (changedProps.ContainsKey(GameModeKey)) {
                if (playersMissingMod.Contains(targetPlayer.ActorNumber)) {
                    playersMissingMod.Remove(targetPlayer.ActorNumber);
                }
            }
        }

        [PunRPC]
        public void ReportTagRPC(Player target, float x, float y, float z, PhotonMessageInfo info)
        {
            Debug.Log("water fight game mode rpc running");

            if (!this.photonView.IsMine || roundOver || !ModRunning) {
                // Debug.Log("not my photonview or round is over");
                // Debug.Log("round over is " + roundOver);
                return;
            }

            var targetPlayer = FindVRRigForPlayer(target);

            if (targetPlayer == null || info.Sender == null) {
                Debug.Log("target player or sender is null");
                return;
            }

            Vector3 hitPoint = new Vector3(x, y, z);

            if (Vector3.Distance(hitPoint, targetPlayer.transform.position) > 5f) {
                return;
            }

            float damage = GetPlayerHealth(info.Sender.ActorNumber) < 1f ? liveDamage : deadDamage;
            if (AddDamage(target.ActorNumber, damage)) { 
                targetPlayer.RPC("PlayTagSound", RpcTarget.All, 0, 0.25f);
            }
        }

        public override float[] LocalPlayerSpeed()
        {
            float[] speeds = { slowJumpMultiplier, slowJumpLimit };
            float health = GetPlayerHealth(PhotonNetwork.LocalPlayer.ActorNumber);
            
            speeds[0] = Mathf.MoveTowards(slowJumpLimit, fastJumpLimit, speedLimitDelta * health);
            speeds[1] = Mathf.MoveTowards(slowJumpMultiplier, fastJumpMultiplier, jumpMultiplyerDelta * health);

            return speeds;
        }

        public override int MyMatIndex(Player myPlayer)
        {
            if (myPlayer == null) {
                return 0;
            }

            float health = GetPlayerHealth(myPlayer.ActorNumber);
            if (health >= 1f) {
                return 3;
            }

            return 0;
        }

        public float GetSlipModifier()
        {
            // float localPlayerHealth = 0;
            if (playerHealth.TryGetValue(PhotonNetwork.LocalPlayer.ActorNumber, out float localHealth)) {
                return SlipModifier * localHealth;
            }

            return 0;
        }

        public float GetPlayerHealth(int actorNumber)
        {
            float health = 1f;
            try {
                if (playerHealth.ContainsKey(actorNumber)) {
                    health = playerHealth[actorNumber];
                }

            } catch(Exception e) {
                Debug.LogError("player health dictionary is missing? " + e.ToString());
            }

            return health;
        }

        private bool AddDamage(int actorNumber, float damage)
        {
            // Debug.Log("adding damage");
            bool result = false;

            try {
                if (playerHealth.TryGetValue(actorNumber, out float health)) {             
                    if (health < 1f) {
                        health = Mathf.Clamp(health + damage, 0f, 1f);
                        playerHealth[actorNumber] = health;
                        result = true;
                        // Debug.Log("new player health is " + health);

                        CheckRoundEnd();
                    }
                }

            } catch (Exception e){
                // Debug.LogError(e.ToString());
            }

            return result;
        }

        private void CheckRoundEnd()
        {
            // this should never happen
            if (!this.photonView.IsMine || roundOver) {
                return;
            }

            // if anyone is still alive, do nothing
           foreach (float healthValue in playerHealth.Values) {
                if (healthValue < 1f) {
                    return;
                }
            }

            roundOver = true;

            foreach (Player player in PhotonNetwork.PlayerList) {

                PhotonView photonView = FindVRRigForPlayer(player);
                if (photonView != null) {
                    photonView.RPC("SetTaggedTime", player, null);
                    photonView.RPC("PlayTagSound", player, 2, 0.25f);
                }
            }

            this.StartCoroutine(RoundEnd(Time.time));      
        }

        private IEnumerator RoundEnd(float timeEnded)
        {
            // Debug.Log("round end coroutine");
            // double make sure
            if (!this.photonView.IsMine) {
                roundOver = false;
                yield break;
            }

            while (Time.time < timeEnded + roundEndCooldown) {
                // Debug.Log("waiting for time cooldown");
                yield return new WaitForSeconds(0.1f);
            }


            Debug.Log("setting everyones health back to 0f");
            List<int> keys = playerHealth.Keys.ToList();

            // set everyones health back to 0
            foreach (var key in keys) {
                // Debug.Log("key = " + key);
                if (playerHealth.ContainsKey(key)) {
                    playerHealth[key] = 0f;
                }
                // Debug.Log("key modified");
            }

            // Debug.Log("setting round over to false");
            roundOver = false;

            yield return null;
        }

        public bool CanHitTarget(Player targetPlayer)
        {
            if (!ModRunning || targetPlayer == null) {
                return false;
            }

            float health = GetPlayerHealth(targetPlayer.ActorNumber);

            if (health < 1f) {
                return true;
            }

            return false;
        }
    }
}
