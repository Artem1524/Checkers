using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Checkers
{
    public class Player : MonoBehaviour
    {
        public ColorType CurrentSide { get; private set; } = ColorType.Black;

        [SerializeField] private ClickHandler _clickHandler;
        [SerializeField] private PhysicsRaycaster _raycaster;

        private List<BaseClickComponent> _whiteChips = new List<BaseClickComponent>();
        private List<BaseClickComponent> _blackChips = new List<BaseClickComponent>();

        private void OnEnable()
        {
            _clickHandler.TurnPerformed += OnTurnPerformed;
            _clickHandler.ChipDestroyed += OnChipDestroyed;
            _clickHandler.GameEnded += OnGameEnded;
        }

        private void OnDisable()
        {
            _clickHandler.TurnPerformed -= OnTurnPerformed;
            _clickHandler.ChipDestroyed -= OnChipDestroyed;
            _clickHandler.GameEnded -= OnGameEnded;
        }

        private void Start()
        {
            foreach (var chip in _clickHandler.Chips)
            {
                if (chip.Color == ColorType.White)
                {
                    _whiteChips.Add(chip);
                }
                else
                {
                    _blackChips.Add(chip);
                }
            }
        }

        private void OnTurnPerformed()
        {
            CurrentSide = CurrentSide == ColorType.Black ? ColorType.White : ColorType.Black;
        }

        private void OnChipDestroyed(BaseClickComponent chip)
        {
            if (_whiteChips.Contains(chip))
            {
                _whiteChips.Remove(chip);
            }
            else
            {
                _blackChips.Remove(chip);
            }

            if (_whiteChips.Count == 0)
            {
                OnGameEnded(ColorType.Black);
            }
            else if (_blackChips.Count == 0)
            {
                OnGameEnded(ColorType.White);
            }
        }

        private void OnGameEnded(ColorType side)
        {
            var teamName = side == ColorType.Black ? "черная" : "белая";
            Debug.Log($"Победила {teamName} команда!");
            _raycaster.enabled = false;
        }
    }
}
