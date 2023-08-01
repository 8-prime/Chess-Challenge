using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class MyBot : IChessBot
    {
        // Piece values: null, pawn, knight, bishop, rook, queen, king
        int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
        ulong[] values =    {   8680820740569200760, 13824261665695437168, 8394018729597438570, 7963065112225482088, 7311441719210115174, 
                                7310878700352475248, 6879365427555374184, 8680820740569200760, 8680820740569200760, 18011561605801242880, 
                                13601353189312935093, 10343222965301904773, 9403364785203083896, 8898396503814797426, 9403092162406021747, 
                                8680820740569200760, 15868519639051563, 4853662768796368236, 6243277331961916824, 8252149912161650056, 
                                8033160056600430450, 7526220316302215789, 7156906411975601771, 3200175186265795432, 5646791984730688305, 
                                7382075402420119379, 7451909818407873371, 7816710223241969259, 7742959389820418923, 7527336333387655784, 
                                6515144560273418328, 7157179051934438474, 7168390707543702898, 7315089821635151190, 7895821240376267639, 
                                8465789406598626679, 8395416191325274235, 8683929107354649984, 8900102941651210361, 6950864474013195369, 
                                7956013853592153191, 8247636214575756654, 8823247078768343163, 8538685803168037498, 8393164369701140082, 
                                8103803669909762925, 7956580127984479589, 7526192780929758572, 10346896576173019031, 10128492919530294168, 
                                8468609610739721348, 7453595421576950378, 6801966475049663848, 6369899035640755552, 6371866087679620165, 
                                7741539898833591909, 9403662821809553020, 9260107272082325114, 9042521600430601590, 8897567472081270906,        
                                8826068438455972464, 8464644659836121709, 8391464472999522422, 8249040325267389290, 7239691988933056409, 
                                7447957048154754463, 8028930218190281377, 7306366287419373433, 8243120524632619638, 7960798949705155196, 
                                6877700844545078905, 8677155002976199252, 8252996454390857863, 7820377155087011448, 7673429650694112895, 
                                8829459461905424786, 7749998532166259849, 7882851297815527548, 7523372499160555105, 6945791258742843995, 
                                5298912041612900994, 10194014800627195235, 8253543877466490984, 7812180084389211742, 6158783937705959507, 
                                7957412315964599653, 8754278914331606910, 7895515283746753154, 4854717032522021740, 8108030214133811584, 
                                9260959406766004354, 8252998653397994362, 7743244163466493808, 7743517928926248306, 7309477995687409004, 
                                5935860240316524377 
                            };


        public Move Think(Board board, Timer timer)
        {
            List<Move> allMoves = board.GetLegalMoves().ToList();

            // Pick a random move to play if nothing better is found
            Random rng = new();
            Move moveToPlay = allMoves[rng.Next(allMoves.Count)];

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

            IOrderedEnumerable<Move> rankedMoves = allMoves
                        .Where(m => !checkmates.Contains(m) && !promotions.Contains(m) && CalculateMoveValue(board, m) > 0)
                        .OrderByDescending(m => CalculateMoveValue(board, m));

            if (rankedMoves.Count() > 0)
            {
                moveToPlay = rankedMoves.First();
            }
            else
            {
                //move pawn up you ass
                Move[] pawnMoves = allMoves.Where(m => m.MovePieceType == PieceType.Pawn && !MoveCreatesTarget(board, m)).ToArray();
                if(pawnMoves.Length > 0)
                    moveToPlay= pawnMoves[rng.Next(pawnMoves.Length)];
            }
            
            return  moveToPlay;
        }

        // Test if this move gives checkmate
        bool MoveIsCheckmate(Board board, Move move)
        {
            board.MakeMove(move);
            bool isMate = board.IsInCheckmate();
            board.UndoMove(move);
            return isMate;
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