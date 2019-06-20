using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArrowRain.KMeanArrow
{

    public static class KMeanVector2
    {

        public static Result DetectCluster(Vector2[] points,
                                           int countOfCluster,
                                           int maxIterationCount)
        {
            var mappings = points.Select(_ => Random.Range(0, countOfCluster))
                .ToArray();

            var zero = Vector2.zero;
            var balances = Enumerable.Range(0, countOfCluster)
                .Select(_ => zero)
                .ToArray();

            var sizeOfCluster = new Dictionary<int, int>();

            Enumerable.Range(0, countOfCluster)
                .ToList()
                .ForEach(i => sizeOfCluster.Add(i, 0));

            var distances = Enumerable.Range(0, points.Length)
                .Select(_ => new float[countOfCluster])
                .ToArray();

            for (var wi = 0; wi < maxIterationCount; ++wi)
            {
                #region Initialize

                for (var i = 0; i < countOfCluster; ++i)
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
                                        + points[i] / count;
                }

                #endregion

                #region CalculateDistances

                for (var pi = 0; pi < points.Length; ++pi)
                {
                    for (var bj = 0; bj < countOfCluster; ++bj)
                    {
                        distances[pi][bj] = (balances[bj] - points[pi]).sqrMagnitude;
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
                    for (var cj = 0; cj < countOfCluster; ++cj)
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

            var radiusList = new float[countOfCluster];
            var radiusCountOfCluster = new Dictionary<int, int>();

            Enumerable.Range(0, countOfCluster)
                .ToList()
                .ForEach(i => radiusCountOfCluster.Add(i, 0));

            for (var i = 0; i < mappings.Length; ++i)
            {
                var mapping = mappings[i];
                radiusCountOfCluster[mapping]++;
                var count = radiusCountOfCluster[mapping];

                radiusList[mapping] = radiusList[mapping]
                                      * ((float)(count - 1) / count)
                                      + (balances[mapping] - points[i]).magnitude / count;
            }

            var result = new Result
            {
                Balances = balances,
                Mappings = mappings,
                Radiuses = radiusList,
            };

            return result;
        }

    }

}
