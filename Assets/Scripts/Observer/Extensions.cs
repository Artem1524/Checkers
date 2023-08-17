using System;

namespace Checkers.Observer
{
    public static class Extensions
    {
        private const string PLAYER_WHITE_NAME = "2";
        private const string PLAYER_BLACK_NAME = "1";
        public static Coordinate ToCoordinate(this (int x, int y) value)
        {
            return new Coordinate(value.x, value.y);
        }

        public static string ToSerializable(this string value, Player player,
                                           CommandType commandType, string destination = "")
        {
            string playerName = (player.CurrentSide == ColorType.White) ? PLAYER_WHITE_NAME : PLAYER_BLACK_NAME;

            switch (commandType)
            {
                case CommandType.Move:      // Player 1 Move from 1:1 to 2:2
                    return $"Player {playerName} {commandType} from {value} to {destination}";
                case CommandType.Click:     // Player 1 Click to 1:1
                    return $"Player {playerName} {commandType} to {value}";
                case CommandType.Remove:    // Player 1 Remove at 1:1
                    return $"Player {playerName} {commandType} at {value}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(commandType), commandType, null);
            }
        }
    }
}
