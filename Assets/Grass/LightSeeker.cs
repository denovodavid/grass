using System;
using UnityEngine;

namespace MuddleClub.MuddleMages.Grass
{
    public class LightSeeker : MonoBehaviour
    {
        public Vector2 range = new Vector2(25, 25);
        public Vector2 speed = new Vector2(0.1f, 0.1f);
        public Vector2 rotSpeed = new Vector2(2, 2);

        private void FixedUpdate()
        {
            var pos = transform.position;
            pos.x = Mathf.Sin(Time.time * speed.x) * range.x;
            pos.z = Mathf.Sin(Time.time * speed.y) * range.y;
            transform.position = pos;

            var rot = transform.rotation;
            var tx = Mathf.Clamp01(Mathf.Sin(Time.time * rotSpeed.x));
            var ty = Mathf.Clamp01(Mathf.Sin(Time.time * rotSpeed.y));
            rot.eulerAngles = new Vector3(
                Mathf.SmoothStep(45, -45, tx * tx * tx) + 90,
                Mathf.SmoothStep(45, -45, ty * ty * ty),
                0
            );
            transform.rotation = rot;
        }
    }
}
