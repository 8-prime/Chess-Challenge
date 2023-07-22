using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class MyBot : IChessBot
    {
        // Piece values: null, pawn, knight, bishop, rook, queen, king
        int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
        int paths = 0;

        public Move Think(Board board, Timer timer)
        {
            paths = 1;
            List<Move> allMoves = board.GetLegalMoves().ToList();

            // Pick a random move to play if nothing better is found
            Random rng = new();

            Move moveToPlay = allMoves[rng.Next(allMoves.Count)];
            int depth = 3;
            float bestMove = float.MinValue;

            foreach(Move move in allMoves)
            {
                paths++;
                board.MakeMove(move);

                float score = MinMax(board, depth, !board.IsWhiteToMove);

                if (score > bestMove)
                {
                    moveToPlay = move;
                    bestMove = score;
                }
                board.UndoMove(move);
            }
            Console.WriteLine(paths);
            return  moveToPlay;
        }
       
        int GetPiecevalue(PieceType pieceType)
        {
            return pieceValues[(int)pieceType];
        }

        int GetColorPiecevalue(Board board, bool isWhite)
        {
            return board.GetAllPieceLists().Where(pl => pl.IsWhitePieceList == isWhite).Select(pl => pl.Count * GetPiecevalue(pl.TypeOfPieceInList)).Sum();
        }

        float CalculateBoardValue(Board board) 
        {
            return GetColorPiecevalue(board, true) - GetColorPiecevalue(board, false);
        }

        float MinMax(Board board, int depth)
        {
            paths++;
            if(depth == 0 || board.IsInCheckmate())
            {
                return CalculateBoardValue(board);
            }

            float max = float.NegativeInfinity;
            foreach(Move move in board.GetLegalMoves())
            {
                board.MakeMove(move);
                float res = -MinMax(board, depth - 1);
                max = Math.Max(max, res);
                board.UndoMove(move);
            }
            return max;
        }
    }
}