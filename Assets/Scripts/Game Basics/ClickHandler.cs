using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

using Checkers.Observer;

namespace Checkers
{
    public class ClickHandler : MonoBehaviour, ISerializable
    {

        [SerializeField]
        private Player _player;
        [SerializeField]
        private Material _chipClickedMaterial;

        private CellComponent[,] _cells;
        private List<CellComponent> _pairs;
        private Vector3 _previousPosition;
        private PathCreator _pathCreator;

        private bool _isReadyToMove = false;
        private BaseClickComponent _selectedCell;

        private IObservable _observer;

        public List<ChipComponent> Chips { get; private set; }

        public event Action TurnPerformed;
        public event Action<BaseClickComponent> ChipDestroyed;
        public event Action<ColorType> GameEnded;
        public event Action StepFinished;

        public void Init(CellComponent[,] cells, List<ChipComponent> chipComponents)
        {
            if (TryGetComponent(out _observer))
            {
                _observer.NextStepReady += OnNextStepReady;
            }

            Chips = chipComponents;
            _pathCreator = new PathCreator(cells, _player);

            _cells = cells;

            foreach (var cell in cells)
            {
                cell.Clicked += OnCellClicked;
            }
        }

        private void OnDisable()
        {
            _observer.NextStepReady -= OnNextStepReady;

            foreach (var cell in _cells)
            {
                cell.Clicked -= OnCellClicked;
            }
        }

        private void OnNextStepReady(Coordinate target)
        {
            if (target.X == -1 &&
                target.Y == -1)
            {
                StepFinished?.Invoke();
                return;
            }

            OnCellClicked(_cells[target.X, target.Y]);
        }

        private void OnCellClicked(BaseClickComponent cell)
        {
            if (_isReadyToMove)
            {
                StartCoroutine(Move(cell));
            }

            ClearCurrentHighlights();

            if (cell.Pair == null)
                return;

            if (_player.CurrentSide != cell.Pair.Color)
            {
                Debug.LogWarning("ƒанна€ фишка принадлежит другому игроку!");
                return;
            }

            _observer?.Serialize(cell.Coordinate.ToString().ToSerializable(_player, CommandType.Click));

            HighlightChip(cell);
            _pairs = FindFreeCells((CellComponent)cell);
            HighlightFreeCells();

            StepFinished?.Invoke();
        }

        private void HighlightFreeCells()
        {
            if (_pairs == null)
                return;

            foreach (CellComponent freeCell in _pairs)
            {
                HighlightFreeCell(freeCell);
            }
        }

        private IEnumerator Move(BaseClickComponent cellToMoving)
        {
            if (!_pairs.Contains(cellToMoving))
            {
                yield break;
            }

            var eventSystem = EventSystem.current;
            eventSystem.gameObject.SetActive(false);

            yield return StartCoroutine(_selectedCell.Pair.Move(cellToMoving));

            _previousPosition = cellToMoving.transform.position;

            _observer?.Serialize(_selectedCell.Coordinate.ToString().ToSerializable(_player, CommandType.Move,
                                  cellToMoving.Coordinate.ToString()));

            TryDestroyChipCandidate();

            if (isPlayerWin(cellToMoving))
                yield break;

            cellToMoving.Pair = _selectedCell.Pair;
            _selectedCell.Pair = null;

            eventSystem.gameObject.SetActive(true);

            TurnPerformed?.Invoke();
            StepFinished?.Invoke();
        }

        private bool isPlayerWin(BaseClickComponent cellToMoving)
        {
            switch (_player.CurrentSide)
            {
                case ColorType.Black when cellToMoving.Coordinate.Y == 7:
                    StepFinished?.Invoke();
                    GameEnded?.Invoke(ColorType.Black);
                    return true;

                case ColorType.White when cellToMoving.Coordinate.Y == 0:
                    StepFinished?.Invoke();
                    GameEnded?.Invoke(ColorType.White);
                    return true;
            }

            return false;
        }

        private List<CellComponent> FindFreeCells(CellComponent cell)
        {
            List<CellComponent> freeCells = _pathCreator.FindFreeCells(cell);

            if (freeCells != null &&
                freeCells.Count > 0)
            {
                _isReadyToMove = true;
                _selectedCell = cell;
            }

            return freeCells;
        }

        private void TryDestroyChipCandidate()
        {
            List<BaseClickComponent> destroyCandidate = _pathCreator.DestroyCandidate;
            if (destroyCandidate.Count != 0)
            {
                DestroyCandidateChip(destroyCandidate);
            }
        }

        private void DestroyCandidateChip(List<BaseClickComponent> destroyCandidate)
        {
            foreach (var cell in destroyCandidate.Where(chip =>
                         Vector3.Distance(chip.transform.position, _previousPosition) < 1.5f))
            {
                ChipDestroyed?.Invoke(cell.Pair);

                _observer?.Serialize(cell.Coordinate.ToString().ToSerializable(_player, CommandType.Remove));

                Destroy(cell.Pair.gameObject);
                return;
            }
        }

        #region Highlights and clear highlights

        private void HighlightChip(BaseClickComponent cell)
        {
            cell.Pair.SetMaterial(_chipClickedMaterial);
        }

        private void HighlightFreeCell(CellComponent freeCell)
        {
            freeCell.IsFreeCellToMove = true;
            freeCell.HighLightFreeCellToMove();
        }

        private void ClearCurrentHighlights()
        {
            _isReadyToMove = false;

            foreach (CellComponent cell in _cells)
            {
                cell.SetMaterial();
                cell.IsFreeCellToMove = false;

                if (cell.Pair != null)
                    cell.Pair.SetMaterial();
            }
        }

        #endregion
    }
}
