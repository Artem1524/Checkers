using System;
using System.Collections.Generic;

using UnityEngine;

namespace Checkers
{
    [RequireComponent(typeof(ClickHandler))]
    public class BoardGenerator : MonoBehaviour
    {
        private static int ROWS = 8;
        private static int COLS = 8;
        private static Dictionary<NeighborType, Coordinate> neighborCoordinateMap = new Dictionary<NeighborType, Coordinate>()
        {
            { NeighborType.TopLeft, new Coordinate(-1, 1) },
            { NeighborType.TopRight, new Coordinate(1, 1) },
            { NeighborType.BottomLeft, new Coordinate(-1, -1) },
            { NeighborType.BottomRight, new Coordinate(1, -1) }
        };

        [SerializeField]
        private CellComponent _cellPrefab;
        [SerializeField]
        private ChipComponent _chipPrefab;

        [SerializeField]
        private Transform _boardTransform;

        private readonly List<ChipComponent> _chips = new List<ChipComponent>();

        private ClickHandler _clickHandler;

        private void Awake()
        {
            GenerateBoard(GetComponent<ClickHandler>());
        }

        private void GenerateBoard(ClickHandler clickHandler)
        {
            var cells = new CellComponent[ROWS, COLS];
            Vector3 beginningOfBoard = GetBoardBeginningPosition();


            for (int i = 0; i < ROWS; i++)
            {
                for (int j = 0; j < COLS; j++)
                {
                    ColorType color = GetColor(i, j);
                    Vector3 cellPosition = beginningOfBoard + _cellPrefab.transform.position + new Vector3(i, 0, j);
                    CellComponent cell = Instantiate(_cellPrefab, cellPosition, Quaternion.identity, transform);

                    cell.SetDefaultMaterial(color == ColorType.White ? cell.WhiteMaterial : cell.BlackMaterial);
                    cell.Coordinate = new Coordinate(i, j);

                    cells[i, j] = cell;

                    if (j >= 3 && j <= 4)
                        continue;

                    if (color == ColorType.Black)
                    {
                        ColorType chipColor = GetChipColorByColPosition(j);

                        CreateChip(cell, chipColor);
                    }
                }
            }

            ConfigureCellsNeighbors(cells);

            clickHandler.Init(cells, _chips);
        }

        /// <summary>
        /// Для каждой клетки на поле добавляем информацию о соседних клетках
        /// </summary>
        private void ConfigureCellsNeighbors(CellComponent[,] cells)
        {
            for (int i = 0; i < ROWS; i++)
            {
                for (int j = 0; j < COLS; j++)
                {
                    Dictionary<NeighborType, CellComponent> neighbors = new Dictionary<NeighborType, CellComponent>();

                    CellComponent cell = cells[i, j];
                    TryAddCellNeighbor(neighbors, cells, cell, NeighborType.TopLeft);
                    TryAddCellNeighbor(neighbors, cells, cell, NeighborType.TopRight);
                    TryAddCellNeighbor(neighbors, cells, cell, NeighborType.BottomLeft);
                    TryAddCellNeighbor(neighbors, cells, cell, NeighborType.BottomRight);

                    cell.Configuration(neighbors);
                    
                }
            }
        }

        private void TryAddCellNeighbor(Dictionary<NeighborType, CellComponent> neighbors, CellComponent[,] cells,
                                    CellComponent cell, NeighborType neighborType)
        {
            Coordinate neighborCellPos = cell.Coordinate + neighborCoordinateMap[neighborType];

            if (! CheckBorders(neighborCellPos, cells))
                return;

            CellComponent neighborCell = cells[neighborCellPos.X, neighborCellPos.Y];

            neighbors.Add(neighborType, neighborCell);
        }

        private bool CheckBorders(Coordinate cellPos, CellComponent[,] cells)
        {
            if (cellPos.X < 0 ||
                cellPos.X > cells.GetUpperBound(0))
                return false;

            if (cellPos.Y < 0 ||
                cellPos.Y > cells.GetUpperBound(1))
                return false;

            return true;
        }

        private void CreateChip(CellComponent cell, ColorType color)
        {
            Vector3 chipPosition = cell.transform.position + _chipPrefab.transform.position;
            ChipComponent chip = Instantiate(_chipPrefab, chipPosition, Quaternion.identity, transform);
            chip.SetDefaultMaterial(color == ColorType.Black ? chip.BlackMaterial : chip.WhiteMaterial);
            chip.Pair = cell;
            cell.Pair = chip;
            chip.Color = color;
            _chips.Add(chip);
        }

        private ColorType GetChipColorByColPosition(int col)
        {
            if (col < 3)
                return ColorType.Black;

            if (col > 4)
                return ColorType.White;

            return default;
        }

        private ColorType GetColor(int i, int j)
        {
            Array colors = typeof(ColorType).GetEnumValues();
            return (ColorType) colors.GetValue((i + j) % 2);
        }

        private Vector3 GetBoardBeginningPosition()
        {
            Vector3 beginningOfBoard = _boardTransform.position - new Vector3(ROWS / 2, 0, COLS / 2);
            Vector3 firstChipPosition = beginningOfBoard + new Vector3(0.5f, 0, 0.5f);
            return firstChipPosition;
        }
    }
}
