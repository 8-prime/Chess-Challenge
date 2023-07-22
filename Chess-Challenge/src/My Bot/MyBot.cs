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
            if(board.GetAllPieceLists().Sum(p => p.Count) < 10)
            {
                depth = 5;
            }
            float bestMove = float.MinValue;

            //Priortize Checkmates
            Move[] checkmates = allMoves.Where(m => MoveIsCheckmate(board, m)).ToArray();
            if (checkmates.Length > 0)
            {
                return checkmates[0];
            }

            //Upgrade pieces to best piece only
            Move[] promotions = allMoves.Where(m => m.IsPromotion).OrderByDescending(m => pieceValues[(int)m.PromotionPieceType]).ToArray();
            if (promotions.Length > 0)
            {
                return promotions[0];
            }

            //Prioritize Checks
            Move[] checks = allMoves.Where(m => MoveIsCheck(board, m) && !MoveCreatesTarget(board, m)).ToArray();
            if (checks.Length > 0)
            {
                return checks[rng.Next(checks.Length)];
            }



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

        // Test if this move gives checkmate
        bool MoveIsCheckmate(Board board, Move move)
        {
            board.MakeMove(move);
            bool isMate = board.IsInCheckmate();
            board.UndoMove(move);
            return isMate;
        }

        bool MoveIsCheck(Board board, Move move)
        {
            board.MakeMove(move);
            bool isCheck = board.IsInCheck();
            board.UndoMove(move);
            return isCheck;
        }
    }
}