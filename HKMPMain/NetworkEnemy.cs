using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Photon;

namespace HKMPMain
{
    public class NetworkEnemy : PunBehaviour, IPunObservable
    {
        Vector3 lastPosition = Vector3.zero;
        string animClip = "";
        int health = 1;

        public tk2dSpriteAnimator anim;
        public HealthManager hp;

        void Update()
        {
            if(health == 0)
            {
                Destroy(gameObject);
            }
            if(!photonView.isMine && MainMod.manager.otherPlayerScene == GameManager.instance.sceneName)
            {
                transform.position = Vector3.Lerp(transform.position, lastPosition, 0.2f);
                //anim.Play(animClip);
            }
            
            if(!photonView.isMine && MainMod.manager.otherPlayerScene != GameManager.instance.sceneName)
            {
                Destroy(gameObject);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if(stream.isWriting)
            {
                stream.SendNext(transform.position);
                stream.SendNext(anim.currentClip.name);
                stream.SendNext(hp.hp);
            }
            else if(stream.isReading)
            {
                lastPosition = (Vector3)stream.ReceiveNext();
                animClip = (string)stream.ReceiveNext();
                health = (int)stream.ReceiveNext();
            }
        }
    }
}
