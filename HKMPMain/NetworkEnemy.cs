using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Photon;
using UnityEngine;

namespace HKMPMain
{
    public class NetworkEnemy : PunBehaviour, IPunObservable
    {
        // Recieved Data
        Vector3 recievedPosition;
        string recievedAnimation;

        // Local Data
        public tk2dSpriteAnimator anim;

        public void Update()
        {
            if(!photonView.isMine)
            {
                transform.position = recievedPosition;
                anim.Play(recievedAnimation);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if(stream.isWriting)
            {
                stream.SendNext(transform.position);
                stream.SendNext(anim.CurrentClip.name);
            }
            else if(stream.isReading)
            {
                recievedPosition = (Vector3)stream.ReceiveNext();
                recievedAnimation = (string)stream.ReceiveNext();
            }
        }

        public void OnDestroy()
        {
            NetworkManager.main.StartCoroutine(UnallocateEnemyID(photonView.viewID));
        }

        public static IEnumerator UnallocateEnemyID(int viewID)
        {
            yield return null;

            PhotonNetwork.UnAllocateViewID(viewID);

            yield break;
        }
    }
}
