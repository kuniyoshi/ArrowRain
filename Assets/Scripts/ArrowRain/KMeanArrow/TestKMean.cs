using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace ArrowRain.KMeanArrow
{

    public class TestKMean : MonoBehaviour
    {

        class Cluster
        {

            public Vector3 Balance { get; private set; }

            public int Id { get; }

            public List<GameObject> Items { get; }

            public Cluster(int id)
            {
                Id = id;
                Items = new List<GameObject>();
            }

            public float CalculateMeanRadius()
            {
                if (!Items.Any())
                {
                    return 0f;
                }

                var sum = Items.Sum(i => (Balance - i.transform.position).magnitude);

                return sum / Items.Count;
            }

            public IEnumerable<Movement> RemoveInvalids(Vector3[] balances)
            {
                var movements = new List<Movement>();

                foreach (var item in Items)
                {
                    var newId = Id;
                    var position = item.transform.position;
                    var minSqrDistance = (balances[Id] - position).sqrMagnitude;

                    for (var i = 0; i < balances.Length; ++i)
                    {
                        if (i == Id)
                        {
                            continue;
                        }

                        var sqrDistance = (balances[i] - position).sqrMagnitude;

                        if (sqrDistance < minSqrDistance)
                        {
                            newId = i;
                            minSqrDistance = sqrDistance;
                        }
                    }

                    if (newId != Id)
                    {
                        movements.Add(new Movement(item, Id));
                    }
                }

                Items.RemoveAll(i => movements.Any(m => m.Item == i));

                return movements;
            }

            public void UpdateBalance()
            {
                var balance = Vector3.zero;

                for (var i = 0; i < Items.Count; ++i)
                {
                    var item = Items[i];
                    var number = i + 1;
                    Assert.IsTrue(number > 0, "number > 0");
                    balance = balance * ((float)(number - 1) / number)
                              + item.transform.position / number;
                }

                Balance = balance;
            }

        }

        class Movement
        {

            public int ClusterId { get; }

            public GameObject Item { get; }

            public Movement(GameObject item, int clusterId)
            {
                Item = item;
                ClusterId = clusterId;
            }

        }

        class ClusterGizmo
        {

            public Vector3 Center { get; }

            public float Radius { get; }

            public ClusterGizmo(Vector3 center, float radius)
            {
                Center = center;
                Radius = radius;
            }

        }

        public int ClusterCount;

        public Transform Destination;

        public float LiftUp;

        public int MaxIterationCount;

        public GameObject NeedlePrefab;

        public float Radius;

        public int SpawnCount;

        List<GameObject> _needles;

        List<ClusterGizmo> _clusters;

        void Awake()
        {
            Assert.IsNotNull(Destination, "Destination != null");
            Assert.IsNotNull(NeedlePrefab, "NeedlePrefab != null");
        }

        void Start()
        {
            _needles = new List<GameObject>();
            _clusters = new List<ClusterGizmo>();
        }

        [ContextMenu("Clear")]
        void Clear()
        {
            _needles.ForEach(Destroy);
            _needles.Clear();
        }

        [ContextMenu("KMean")]
        void KMean()
        {
            if (_needles.Count <= ClusterCount)
            {
                return;
            }

            var clusters = Enumerable.Range(0, ClusterCount)
                .Select(id => new Cluster(id))
                .ToArray();

            foreach (var needle in _needles)
            {
                var index = UnityEngine.Random.Range(0, clusters.Length);
                clusters[index].Items.Add(needle);
            }

            var balances = new Vector3[clusters.Length];
            var wholeMovements = new List<Movement>();

            for (var wi = 0; wi < MaxIterationCount; ++wi)
            {
                foreach (var cluster in clusters)
                {
                    cluster.UpdateBalance();
                }

                for (var i = 0; i < clusters.Length; ++i)
                {
                    balances[i] = clusters[i].Balance;
                }

                wholeMovements.Clear();

                foreach (var cluster in clusters)
                {
                    var movements = cluster.RemoveInvalids(balances);
                    wholeMovements.AddRange(movements);
                }

                foreach (var movement in wholeMovements)
                {
                    clusters[movement.ClusterId].Items.Add(movement.Item);
                }

                if (!wholeMovements.Any())
                {
                    break;
                }
            }

            _clusters.Clear();
            _clusters.AddRange(
                clusters.Select(c => new ClusterGizmo(c.Balance, c.CalculateMeanRadius()))
            );
        }

        [ContextMenu("KMeans2")]
        void KMeans2()
        {
            if (_needles.Count <= ClusterCount)
            {
                return;
            }

            var positions = _needles.Select(n =>
                {
                    var p = n.transform.position;

                    return new Vector2(p.x, p.z);
                })
                .ToArray();

            var mappings = positions.Select(_ => UnityEngine.Random.Range(0, ClusterCount))
                .ToArray();

            DebugSummarize(mappings);

            var zero = Vector2.zero;
            var balances = Enumerable.Range(0, ClusterCount)
                .Select(_ => zero)
                .ToArray();

            var sizeOfCluster = new Dictionary<int, int>();

            Enumerable.Range(0, ClusterCount)
                .ToList()
                .ForEach(i => sizeOfCluster.Add(i, 0));

            var distances = Enumerable.Range(0, positions.Length)
                .Select(_ => new float[ClusterCount])
                .ToArray();

            for (var wi = 0; wi < MaxIterationCount; ++wi)
            {
                #region Initialize

                for (var i = 0; i < ClusterCount; ++i)
                {
                    sizeOfCluster[i] = 0;
                }

                #endregion

                #region CalculateBalances

                for (var i = 0; i < mappings.Length; ++i)
                {
                    var mapping = mappings[i];
                    sizeOfCluster[mapping]++;
                    var count = sizeOfCluster[mapping];

                    balances[mapping] = balances[mapping] * ((float)(count - 1) / count)
                                        + positions[i] / count;
                }

                #endregion

                #region CalculateDistances

                for (var pi = 0; pi < positions.Length; ++pi)
                {
                    for (var bj = 0; bj < ClusterCount; ++bj)
                    {
                        distances[pi][bj] = (balances[bj] - positions[pi]).sqrMagnitude;
                    }
                }

                #endregion

                #region ReMapping

                var mappingCount = 0;

                for (var mi = 0; mi < mappings.Length; ++mi)
                {
                    var newMapping = mappings[mi];
                    var minDistance = distances[mi][mappings[mi]];

                    // search index of min distance
                    for (var cj = 0; cj < ClusterCount; ++cj)
                    {
                        var distance = distances[mi][cj];

                        if (distance < minDistance)
                        {
                            newMapping = cj;
                            minDistance = distance;
                        }
                    }

                    if (newMapping != mappings[mi])
                    {
                        mappingCount++;
                        mappings[mi] = newMapping;
                    }
                }

                #endregion

                #region TestCanFinish

                if (mappingCount == 0)
                {
                    Debug.Log($"Completely clustered");

                    break;
                }

                #endregion
            }

            #region UpdateGizmoData

            var radiusList = new float[ClusterCount];
            var radiusCountOfCluster = new Dictionary<int, int>();

            Enumerable.Range(0, ClusterCount)
                .ToList()
                .ForEach(i => radiusCountOfCluster.Add(i, 0));

            for (var i = 0; i < mappings.Length; ++i)
            {
                var mapping = mappings[i];
                radiusCountOfCluster[mapping]++;
                var count = radiusCountOfCluster[mapping];

                radiusList[mapping] = radiusList[mapping]
                                      * ((float)(count - 1) / count)
                                      + (balances[mapping] - positions[i]).magnitude / count;
            }

            DebugSummarize(mappings);

            _clusters.Clear();
            _clusters.AddRange(
                Enumerable.Range(0, ClusterCount)
                    .Select(i =>
                    {
                        var b = balances[i];
                        var p = new Vector3(b.x, 0f, b.y);

                        return new ClusterGizmo(p, radiusList[i]);
                    })
            );

            #endregion
        }

        void DebugSummarize(int[] mappings)
        {
            var counts = Enumerable.Range(0, ClusterCount)
                .Select(_ => 0)
                .ToArray();

            foreach (var mapping in mappings)
            {
                counts[mapping]++;
            }

            Debug.Log(string.Join(", ", counts));
        }

        [ContextMenu("Spawn")]
        void Spawn()
        {
            var destination = Destination.position;
            var needles = Enumerable.Range(0, SpawnCount)
                .Select(_ =>
                {
                    var variance = UnityEngine.Random.insideUnitCircle * Radius;
                    var point = destination + new Vector3(
                                    variance.x,
                                    LiftUp,
                                    variance.y
                                );

                    var newGameObject = Instantiate(
                        NeedlePrefab,
                        point,
                        Quaternion.identity,
                        transform
                    );

                    return newGameObject;
                })
                .ToList();

            _needles.AddRange(needles);
        }

        void OnDrawGizmos()
        {
            if (_clusters == null)
            {
                return;
            }

            Gizmos.color = Color.blue;

            foreach (var cluster in _clusters)
            {
                Gizmos.DrawWireSphere(cluster.Center, cluster.Radius);
            }
        }

    }

}
