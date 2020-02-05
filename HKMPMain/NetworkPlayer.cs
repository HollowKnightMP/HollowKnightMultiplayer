using System;
using Photon;
using UnityEngine;
using System.Collections.Generic;

namespace HKMPMain
{
    public class NetworkPlayer : PunBehaviour, IPunObservable
    {
        public Collider col;
        public List<string> enemyNames;
        public List<int> enemyIDs;

        // OTHER PLAYER VALUES
        public MeshRenderer renderer;
        public tk2dSpriteAnimator anim;
        public PlayMakerFSM takeDamageEffect;

        // RECIEVED DATA
        public Vector3 position;
        public string levelName;
        public int health = 5;
        public string animName;
        public bool facingRight = false;
        public bool canTakeDmg = true;
        public bool isSceneHost = false;

        [Serializable]
        public class SerializedEnemies
        {
            public List<string> enemyNames;
            public List<int> enemyIDs;
        }

        void Update()
        {
            if(!photonView.isMine)
            {
                if(levelName != GameManager.instance.GetSceneNameString())
                {
                    renderer.enabled = false;
                    return;
                }
                else
                {
                    renderer.enabled = true;
                }

                if (Vector3.Distance(transform.position, position) < 10f)
                {
                    transform.position = Vector3.Lerp(transform.position, position, 0.2f);
                }
                else
                {
                    transform.position = position;
                }

                anim.Play(animName);
                transform.localScale = new Vector3(facingRight ? -1 : 1, 1, 1);
            }
        }
        
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if(stream.isWriting)
            {
                stream.SendNext(HeroController.instance.transform.position + (Vector3)HeroController.instance.rb2d.velocity * Time.deltaTime);
                stream.SendNext(GameManager.instance.GetSceneNameString());
                stream.SendNext(HeroController.instance.playerData.health);

                stream.SendNext(HeroController.instance.animCtrl.animator.currentClip.name);
                stream.SendNext(HeroController.instance.cState.facingRight);

                stream.SendNext(HeroController.instance.CanTakeDamage());
            }
            else if(stream.isReading)
            {
                position = (Vector3)stream.ReceiveNext();
                levelName = (string)stream.ReceiveNext();
                health = (int)stream.ReceiveNext();

                animName = (string)stream.ReceiveNext();
                facingRight = (bool)stream.ReceiveNext();

                canTakeDmg = (bool)stream.ReceiveNext();
            }
        }
    }
}