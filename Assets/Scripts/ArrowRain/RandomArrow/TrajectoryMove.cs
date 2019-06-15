using UnityEngine;

namespace ArrowRain.RandomArrow
{

    public class TrajectoryMove : MonoBehaviour
    {

        class ShortTermMemory
        {

            public Vector3 Acceleration;

            public Vector3 Destination;

            public float DestroyedAt;

            public bool DidDestroy;

            public bool IsMoving;

            public Vector3 LastPosition;

            public Vector3 Velocity;

            public float WillEndedAt;

        }

        static float CalcDuration(Vector3 diff, Vector3 velocity)
        {
            var x = Mathf.Approximately(velocity.x, 0f)
                ? 0f
                : diff.x / velocity.x;

            var y = Mathf.Approximately(velocity.y, 0f)
                ? 0f
                : diff.y / velocity.y;

            var z = Mathf.Approximately(velocity.z, 0f)
                ? 0f
                : diff.z / velocity.z;

            var max = Mathf.Max(x, y, z);

            return max;
        }

        ShortTermMemory Memory { get; } = new ShortTermMemory();

        void Update()
        {
            if (Memory.DidDestroy)
            {
                return;
            }

            if (Time.time > Memory.DestroyedAt)
            {
                Memory.DidDestroy = true;
                Destroy(gameObject);

                return;
            }

            if (!Memory.IsMoving)
            {
                return;
            }

            if (Time.time > Memory.WillEndedAt)
            {
                Memory.IsMoving = false;
                transform.position = Memory.Destination;

                return;
            }

            Memory.Velocity = Memory.Velocity + Memory.Acceleration * Time.deltaTime;
            var newPosition = transform.position
                              + Memory.Velocity * Time.deltaTime;

            transform.position = newPosition;

            var deltaPosition = transform.position - Memory.LastPosition;
            Memory.LastPosition = newPosition;
            transform.rotation = Quaternion.LookRotation(deltaPosition);
        }

        public void FireTo(Vector3 destination, Vector3 velocity)
        {
            Memory.Destination = destination;
            Memory.Velocity = velocity;
            Memory.IsMoving = true;

            var position = transform.position;
            Memory.LastPosition = position;

            var diff = destination - position;
            var duration = CalcDuration(diff, velocity);
            var acceleration = (diff - velocity * duration) * 2f
                               / Mathf.Pow(duration, 2f);

            Memory.Acceleration = acceleration;
            Memory.WillEndedAt = Time.time + duration;

            Memory.DestroyedAt = Memory.WillEndedAt + 5f;
        }

    }

}
