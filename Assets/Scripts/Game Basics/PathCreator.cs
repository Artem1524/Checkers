using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Checkers
{
    public class PathCreator
    {
        private readonly CellComponent[,] _cells;
        private readonly Player _player;

        public List<BaseClickComponent> DestroyCandidate { get; private set; } = new();

        public PathCreator(CellComponent[,] cells, Player player)
        {
            _cells = cells;
            _player = player;
        }

        public List<CellComponent> FindFreeCells(CellComponent cell)
        {
            DestroyCandidate.Clear();

            List<CellComponent> pairs = new List<CellComponent>();

            if (_player.CurrentSide == ColorType.Black)
            {
                TryAddNeighbors(pairs, cell, NeighborType.TopLeft, NeighborType.TopRight);
            }
            else if (_player.CurrentSide == ColorType.White)
            {
                TryAddNeighbors(pairs, cell, NeighborType.BottomLeft, NeighborType.BottomRight);
            }

            pairs = FindDestroyCandidates(pairs, cell);

            return pairs;
        }

        private List<CellComponent> FindDestroyCandidates(List<CellComponent> pairs, CellComponent selectedCell)
        {
            List<CellComponent> freeCells = new List<CellComponent>(
                                                pairs.Where(c => c.Pair == null)
                                                .ToList());

            List<CellComponent> cellsWithOpponentChip = new List<CellComponent>(
                                                pairs.Where(c => c.Pair != null && c.Pair.Color != _player.CurrentSide)
                                                .ToList());

            bool isFreeCellsOverOpponentChip;

            foreach (CellComponent cell in cellsWithOpponentChip)
            {
                isFreeCellsOverOpponentChip = false;

                foreach (CellComponent neighborCell in cell.GetNeighbors(_player.CurrentSide)) // Проверяем клетки за фишкой соперника
                {
                    if (neighborCell.Pair == null && Vector3.Distance(selectedCell.transform.position, neighborCell.transform.position) > 2.8f)
                        freeCells.Add(neighborCell); // Если клетка свободна и она находится по диагонали, то добавляем в список

                    isFreeCellsOverOpponentChip = true;
                }

                if (isFreeCellsOverOpponentChip)
                    DestroyCandidate.Add(cell);
            }

            return freeCells;
        }

        private void TryAddNeighbors(List<CellComponent> pairs, CellComponent cell, params NeighborType[] neighborTypes)
        {
            foreach (NeighborType type in neighborTypes)
            {
                CellComponent neighborCell = cell.GetNeighbor(type);

                if (neighborCell != null)
                    pairs.Add(neighborCell);
            }
        }
    }
}
