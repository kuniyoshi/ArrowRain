using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace ArrowRain.KMeanArrow
{

    public class KMeanArrowController : MonoBehaviour
    {

        public GameObject ArrowPrefab;

        public GameObject Destination;

        public int Count;

        public float Radius;

        public Vector3 Velocity;

        public float VarianceCoef;

        public int MaxIterationCount;

        public int SampleSize;

        void Awake()
        {
            Assert.IsNotNull(ArrowPrefab, "ArrowPrefab != null");
            Assert.IsNotNull(Destination, "Destination != null");
        }

        [ContextMenu("Fire")]
        void Fire()
        {
            var positions = Enumerable.Range(0, SampleSize)
                .Select(_ => Random.insideUnitCircle * Radius)
                .ToArray();

            var result = KMeanVector2.DetectCluster(
                positions,
                Count,
                MaxIterationCount
            );

            var sourcePoint = transform.position;
            var destinationPoint = Destination.transform.position;

            result.Balances
                .Zip(result.Radiuses, (balance, radius) => new {Balance = balance, Radius = radius})
                .ToList()
                .ForEach(d =>
                {
                    var variance = d.Radius * VarianceCoef/**/ * Random.insideUnitCircle;
                    var destination = destinationPoint
                                      + new Vector3(
                                          d.Balance.x,
                                          0f,
                                          d.Balance.y
                                      )
                                      + new Vector3(
                                          variance.x,
                                          0f,
                                          variance.y
                                      );

                    var newGameObject = Instantiate(ArrowPrefab);
                    newGameObject.transform.position = sourcePoint;

                    var arrow = newGameObject.AddComponent<TrajectoryMove>();
                    arrow.FireTo(destination, Velocity);
                });
        }

    }

}
