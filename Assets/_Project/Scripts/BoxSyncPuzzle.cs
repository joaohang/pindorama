using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Orquestra um conjunto de caixas + marcações.
    /// - Todas as marcações ocupadas ao mesmo tempo => sucesso e desliga os lasers.
    /// - Uma marcação ocupada sozinha por mais de simultaneousWindow => falha.
    /// - Caixa errada em uma marcação => falha imediata.
    /// Na falha, todas as caixas explodem e voltam aos spawns.
    /// </summary>
    public class BoxSyncPuzzle : MonoBehaviour
    {
        [SerializeField] private BoxSnapTarget[] targets;
        [SerializeField] private BoxIdentity[] boxes;
        [SerializeField] private LaserHazard[] lasersToDisable;
        [SerializeField] private float simultaneousWindow = 3f;
        [SerializeField] private float failureCooldown = 0.75f;

        private float oneOccupiedSince = -1f;
        private float lastFailureTime = -999f;
        private bool solved;
        private bool resolving;

        public bool Solved { get { return solved; } }

        public void Configure(BoxSnapTarget[] targetList, BoxIdentity[] boxList,
                              LaserHazard[] lasers, float window)
        {
            targets = targetList;
            boxes = boxList;
            lasersToDisable = lasers;
            simultaneousWindow = window;
        }

        private void OnEnable()
        {
            Subscribe(true);
        }

        private void OnDisable()
        {
            Subscribe(false);
        }

        private void Subscribe(bool on)
        {
            if (targets == null)
                return;

            for (int i = 0; i < targets.Length; i++)
            {
                BoxSnapTarget target = targets[i];
                if (target == null)
                    continue;

                if (on)
                    target.WrongBoxEntered += OnWrongBox;
                else
                    target.WrongBoxEntered -= OnWrongBox;
            }
        }

        private void OnWrongBox(BoxIdentity box)
        {
            Fail();
        }

        private void Update()
        {
            if (solved || resolving || targets == null || targets.Length == 0)
                return;

            int occupied = 0;
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] != null && targets[i].IsOccupied)
                    occupied++;
            }

            if (occupied == targets.Length)
            {
                Solve();
                return;
            }

            if (occupied > 0)
            {
                if (oneOccupiedSince < 0f)
                    oneOccupiedSince = Time.time;
                else if (Time.time - oneOccupiedSince > simultaneousWindow)
                    Fail();
            }
            else
            {
                oneOccupiedSince = -1f;
            }
        }

        private void Solve()
        {
            solved = true;

            if (lasersToDisable == null)
                return;

            for (int i = 0; i < lasersToDisable.Length; i++)
            {
                if (lasersToDisable[i] != null)
                    lasersToDisable[i].SetActive(false);
            }
        }

        public void Fail()
        {
            if (solved || resolving || Time.time < lastFailureTime + failureCooldown)
                return;

            resolving = true;
            lastFailureTime = Time.time;

            if (boxes != null)
            {
                for (int i = 0; i < boxes.Length; i++)
                {
                    if (boxes[i] != null)
                        boxes[i].ExplodeAndReset();
                }
            }

            if (targets != null)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    if (targets[i] != null)
                        targets[i].Clear();
                }
            }

            oneOccupiedSince = -1f;
            resolving = false;
        }

        /// <summary>
        /// Reset manual (R). Mantém o laser desligado se o puzzle já foi resolvido.
        /// </summary>
        public void ResetPuzzle()
        {
            if (solved)
                return;

            if (boxes != null)
            {
                for (int i = 0; i < boxes.Length; i++)
                {
                    if (boxes[i] != null)
                        boxes[i].ResetBox();
                }
            }

            if (targets != null)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    if (targets[i] != null)
                        targets[i].Clear();
                }
            }

            oneOccupiedSince = -1f;
        }
    }
}
