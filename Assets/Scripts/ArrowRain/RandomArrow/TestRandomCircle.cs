using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace ArrowRain.RandomArrow
{

    public class TestRandomCircle : MonoBehaviour
    {

        public GameObject NeedlePrefab;

        public float Radius;

        public int Count;

        public float LiftUp;

        List<GameObject> _needles;

        void Awake()
        {
            Assert.IsNotNull(NeedlePrefab, "NeedlePrefab != null");
        }

        void Start()
        {
            _needles = new List<GameObject>();
        }

        [ContextMenu("Clear")]
        void Clear()
        {
            _needles.ForEach(Destroy);
            _needles.Clear();
        }

        [ContextMenu("Spawn")]
        void Spawn()
        {
            var needles = Enumerable.Range(0, Count)
                .Select(_ =>
                    {
                        var newGameObject = Instantiate(
                            NeedlePrefab,
                            transform
                        );

                        var rand = Random.insideUnitCircle * Radius;
                        var position = new Vector3(
                            rand.x,
                            LiftUp,
                            rand.y
                        );

                        newGameObject.transform.localPosition = position;

                        return newGameObject;
                    }
                );

            _needles.AddRange(needles);
        }

    }

}
