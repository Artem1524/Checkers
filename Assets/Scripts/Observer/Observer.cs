using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Checkers.Observer
{
    [RequireComponent(typeof(ISerializable))]
    public class Observer : MonoBehaviour, IObservable
    {
        private static readonly string PLAYER_COMMAND_PATTERN = @"Player (\d+) (Move|Click|Remove)";
        private static readonly string COORDINATE_PATTERN = @"(\d+):(\d+)";

        [SerializeField]
        private string _fileName;

        [SerializeField]
        private float _delayBetweenActions;

        [SerializeField]
        private bool _needSerialize;

        [SerializeField]
        private bool _needDeserialize;

        [SerializeField]
        private PhysicsRaycaster _raycaster;

        private ISerializable _handler;
        private List<string> _output;

        public event Action<Coordinate> NextStepReady;

        private void Awake()
        {
            _handler = GetComponent<ISerializable>();
        }

        private void Start()
        {
            if (_needSerialize && !_needDeserialize)
                File.Delete(GetFileNameWithExtension());

            if (!_needSerialize && _needDeserialize)
            {
                _raycaster.enabled = false;
                string output = Deserialize();
                _output = output.Split(Environment.NewLine).ToList();
                OnStepFinished();
            }
        }

        private void OnEnable()
        {
            _handler.StepFinished += OnStepFinished;
        }

        private void OnDisable()
        {
            _handler.StepFinished -= OnStepFinished;
        }

        public async Task Serialize(string input)
        {
            if (!_needSerialize)
                return;

            await using FileStream fileStream = new FileStream(GetFileNameWithExtension(), FileMode.Append);
            await using StreamWriter streamWriter = new StreamWriter(fileStream);

            await streamWriter.WriteLineAsync(input);
        }

        private string Deserialize()
        {
            if (!File.Exists(GetFileNameWithExtension()))
                return null;

            using FileStream fileStream = new FileStream(GetFileNameWithExtension(), FileMode.Open);
            using StreamReader streamReader = new StreamReader(fileStream);

            StringBuilder builder = new StringBuilder();

            while (!streamReader.EndOfStream)
            {
                builder.AppendLine(streamReader.ReadLine());
            }

            return builder.ToString();
        }

        private void OnStepFinished()
        {
            if (!_needDeserialize && _needSerialize)
            {
                return;
            }

            string stepInput = _output[0];

            if (string.IsNullOrWhiteSpace(stepInput))
            {
                _needSerialize = true;
                _needDeserialize = false;
                Debug.Log("Game repeated!");
                _raycaster.enabled = true;
                return;
            }

            StartCoroutine(Repeat(stepInput));
            _output.RemoveAt(0);
        }

        private IEnumerator Repeat(string input)
        {
            yield return new WaitForSeconds(_delayBetweenActions);

            Coordinate destinationPosition = default;

            Match playerCommandMatch = Regex.Match(input, PLAYER_COMMAND_PATTERN);
            int playerName = int.Parse(playerCommandMatch.Groups[1].Value);
            string playerCommand = playerCommandMatch.Groups[2].Value;

            MatchCollection coordinateMatches = Regex.Matches(input, COORDINATE_PATTERN);

            Coordinate originPosition = (
                     int.Parse(coordinateMatches[0].Groups[1].Value),
                     int.Parse(coordinateMatches[0].Groups[2].Value)
                    ).ToCoordinate();

            if (playerCommand == CommandType.Move.ToString())
            {
                destinationPosition = (
                     int.Parse(coordinateMatches[1].Groups[1].Value),
                     int.Parse(coordinateMatches[1].Groups[2].Value)
                    ).ToCoordinate();
            }

            switch (playerCommand)
            {
                case "Move":
                    Debug.Log($"Player {playerName} {playerCommand} from {originPosition} to {destinationPosition}");
                    NextStepReady?.Invoke(destinationPosition);
                    break;

                case "Click":
                    Debug.Log($"Player {playerName} {playerCommand} to {originPosition}");
                    NextStepReady?.Invoke(originPosition);
                    break;

                case "Remove":
                    Debug.Log($"Player {playerName} {playerCommand} at {originPosition}");
                    NextStepReady?.Invoke(new Coordinate(-1, -1));
                    break;

                default:
                    throw new NullReferenceException("CommandType is null!");
            }
        }

        private string GetFileNameWithExtension()
        {
            return _fileName + ".txt";
        }
    }
}
