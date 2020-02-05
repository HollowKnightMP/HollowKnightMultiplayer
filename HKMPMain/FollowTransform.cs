using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HKMPMain
{
    public class FollowTransform : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = Vector3.zero;
        public Quaternion rotation = Quaternion.identity;

        public void Update()
        {
            if(target)
            {
                transform.position = target.position + offset;
                transform.rotation = rotation;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
