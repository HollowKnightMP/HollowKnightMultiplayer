using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Photon;

namespace HKMPMain
{
    public class NetworkPlayerController : PunBehaviour, IPunObservable
    {
        // Recieved Data
        string clipName;
        bool isVisible;
        bool facingRight;

        Vector3 lastPosition = Vector3.zero;

        // Other Data
        public tk2dSpriteAnimator anim;
        public MeshRenderer renderer;

        public NailSlash slashUp;
        public NailSlash slashDown;
        public NailSlash slashNormal;

        public ParticleSystem dash;
        public ParticleSystem shadowDash;
        public ParticleSystem jumpFeathers;
        public GameObject jumpWings;
        public GameObject dreamNailVFX;

        public void Update()
        {
            if(!photonView.isMine)
            {
                // Teleport if too far away
                if (isVisible)
                {
                    if (Vector3.Distance(transform.position, lastPosition) > 10f)
                    {
                        transform.position = lastPosition;
                    }
                    else
                    {
                        transform.position = Vector3.Lerp(transform.position, lastPosition, 0.2f);
                    }
                }

                anim.enabled = true;
                renderer.enabled = isVisible;

                anim.Play(clipName);

                if(clipName == "Dash" || clipName == "Dash Down")
                {
                    var em = dash.emission;
                    em.enabled = true;
                }
                else
                {
                    var em = dash.emission;
                    em.enabled = false;
                }

                if(clipName == "Shadow Dash" || clipName == "Shadow Dash Sharp" || clipName == "Shadow Dash Down" || clipName == "Shadow Dash Down Sharp")
                {
                    var em = shadowDash.emission;
                    em.enabled = true;
                }
                else
                {
                    var em = shadowDash.emission;
                    em.enabled = false;
                }

                if(clipName == "Double Jump")
                {
                    jumpFeathers.Play();
                    jumpWings.SetActive(true);
                }
                else
                {
                    jumpWings.SetActive(false);
                }

                Vector3 scale = Vector3.one;
                scale.x = facingRight ? -1 : 1;
                transform.localScale = scale;
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if(stream.isWriting)
            {
                stream.SendNext(GameManager.instance.GetSceneNameString());
                stream.SendNext(HeroController.instance.transform.position + ((Vector3)HeroController.instance.rb2d.velocity*Time.deltaTime));

                stream.SendNext(HeroController.instance.animCtrl.animator.currentClip.name);
                stream.SendNext(HeroController.instance.cState.facingRight);
            }
            else if(stream.isReading)
            {
                // Check if players are in the same scene
                string scene = (string)stream.ReceiveNext();
                isVisible = (scene == GameManager.instance.GetSceneNameString());
                Vector3 pos = (Vector3)stream.ReceiveNext();
                //Console.WriteLine($"[HollowKnightMP] Got position {pos} from player {photonView.owner.NickName}");
                lastPosition = pos;

                clipName = stream.ReceiveNext() as string;
                facingRight = (bool)stream.ReceiveNext();
            }
        }
    }
}
