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

        public Move Think(Board board, Timer timer)
        {
            List<Move> allMoves = board.GetLegalMoves().ToList();

            // Pick a random move to play if nothing better is found
            Random rng = new();

            Move moveToPlay = allMoves[rng.Next(allMoves.Count)];
            int depth = 3;
            float bestMove = float.MinValue;

            // Order moves from best to worst to improve pruning performance
            foreach (Move move in allMoves.OrderByDescending(m => CalculateMoveValue(board,m)))
            {
                board.MakeMove(move);

                float score = -MinMax(board, depth, float.MinValue, float.MaxValue);

                if (score > bestMove)
                {
                    moveToPlay = move;
                    bestMove = score;
                }
                board.UndoMove(move);
            }
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
            return GetColorPiecevalue(board, true) - GetColorPiecevalue(board, false) * (board.IsWhiteToMove ? 1 : -1);
        }

        float MinMax(Board board, int depth, float alpha, float beta)
        {
            if(depth == 0 || board.IsInCheckmate())
            {
                return CalculateBoardValue(board);
            }

            float max = float.NegativeInfinity;
            // Order moves from best to worst to improve pruning performance
            foreach(Move move in board.GetLegalMoves().OrderByDescending(m => CalculateMoveValue(board, m)))
            {
                board.MakeMove(move);
                max = Math.Max(-MinMax(board, depth - 1, -beta, -alpha), max);
                alpha = Math.Max(alpha, max);
                board.UndoMove(move);
                if(alpha >= beta)
                {
                    break;
                }
            }
            return max;
        }


        bool MoveCreatesTarget(Board board, Move move)
        {
            board.MakeMove(move);
            Move[] enemyMoves = board.GetLegalMoves();
            bool isTarget = enemyMoves.Any(m => m.TargetSquare == move.TargetSquare);
            board.UndoMove(move);

            return isTarget;
        }


        int CalculateMoveValue(Board board, Move move)
        {
            Piece capturedPiece = board.GetPiece(move.TargetSquare);
            int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

            int cost = !MoveCreatesTarget(board, move) ? 0 : pieceValues[(int)move.MovePieceType];

            return capturedPieceValue - cost;
        }
    }
}