using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace ArrowRain.RandomArrow
{

    public class RandomArrowController : MonoBehaviour
    {

        static Vector3 ClampVelocity(Vector3 velocity)
        {
            velocity.x = velocity.x < 0f ? 0f : velocity.x;
            velocity.y = velocity.y < 0f ? 0f : velocity.y;
            velocity.z = velocity.z < 0f ? 0f : velocity.z;

            return velocity;
        }

        public GameObject ArrowPrefab;

        public GameObject Destination;

        public int Count;

        public Vector3 Velocity;

        public Vector3 Variance;

        public float Radius;

        void Awake()
        {
            Assert.IsNotNull(ArrowPrefab, "ArrowPrefab != null");
        }

        [ContextMenu("Fire")]
        void Fire()
        {
            var sourcePoint = transform.position;
            var arrows = Enumerable.Range(0, Count)
                .Select(_ =>
                {
                    var newGameObject = Instantiate(ArrowPrefab);
                    newGameObject.transform.position = sourcePoint;
                    var mover = newGameObject.AddComponent<TrajectoryMove>();

                    return mover;
                })
                .ToList();

            var position = Destination.transform.position;

            var destinations = Enumerable.Range(0, arrows.Count)
                .Select(_ =>
                {
                    var variance = Random.insideUnitCircle * Radius;

                    return position + new Vector3(variance.x, 0f, variance.y);
                })
                .ToList();

            arrows.Zip(destinations, (a, d) => new {Arrow = a, Destination = d})
                .ToList()
                .ForEach(d =>
                {
                    var variance = new Vector3(
                        Random.Range(-Variance.x, Variance.x),
                        Random.Range(-Variance.y, Variance.y),
                        Random.Range(-Variance.z, Variance.z)
                    );

                    var velocity = ClampVelocity(Velocity + variance);

                    d.Arrow.FireTo(d.Destination, velocity);
                });
        }

    }

}
