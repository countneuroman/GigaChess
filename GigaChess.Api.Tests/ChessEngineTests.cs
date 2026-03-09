using GigaChess.Api.Engine;
using GigaChess.Api.Helpers;
using GigaChess.Api.Models;

namespace GigaChess.Api.Tests;

public class ChessEngineTests
{
    private readonly ChessEngine _engine = new();

    private static GameState FromFen(string fen)
    {
        var state = new GameState { GameId = "test" };
        BoardHelper.FromFen(fen, state);
        return state;
    }

    private static GameState StartingPosition() =>
        FromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

    // --- Pawn ---

    [Fact]
    public void Pawn_ForwardOne_Legal()
    {
        var state = StartingPosition();
        Assert.True(_engine.IsMoveLegal(state, "e2", "e3"));
    }

    [Fact]
    public void Pawn_ForwardTwo_FromStart_Legal()
    {
        var state = StartingPosition();
        Assert.True(_engine.IsMoveLegal(state, "e2", "e4"));
    }

    [Fact]
    public void Pawn_ForwardTwo_NotFromStart_Illegal()
    {
        var state = FromFen("rnbqkbnr/pppppppp/8/8/8/4P3/PPPP1PPP/RNBQKBNR w KQkq - 0 1");
        Assert.False(_engine.IsMoveLegal(state, "e3", "e5"));
    }

    [Fact]
    public void Pawn_ForwardOne_Blocked_Illegal()
    {
        var state = FromFen("rnbqkbnr/pppp1ppp/8/8/8/4p3/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        Assert.False(_engine.IsMoveLegal(state, "e2", "e3"));
    }

    [Fact]
    public void Pawn_ForwardTwo_PathBlocked_Illegal()
    {
        var state = FromFen("rnbqkbnr/pppppppp/8/8/8/4p3/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        Assert.False(_engine.IsMoveLegal(state, "e2", "e4"));
    }

    [Fact]
    public void Pawn_DiagonalCapture_Legal()
    {
        var state = FromFen("rnbqkbnr/pppp1ppp/8/4p3/3P4/8/PPP1PPPP/RNBQKBNR w KQkq - 0 1");
        Assert.True(_engine.IsMoveLegal(state, "d4", "e5"));
    }

    [Fact]
    public void Pawn_DiagonalNoCapture_Illegal()
    {
        var state = StartingPosition();
        Assert.False(_engine.IsMoveLegal(state, "e2", "d3"));
    }

    [Fact]
    public void Pawn_Backward_Illegal()
    {
        var state = FromFen("rnbqkbnr/pppppppp/8/8/8/4P3/PPPP1PPP/RNBQKBNR w KQkq - 0 1");
        Assert.False(_engine.IsMoveLegal(state, "e3", "e2"));
    }

    [Fact]
    public void BlackPawn_ForwardOne_Legal()
    {
        var state = FromFen("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq - 0 1");
        Assert.True(_engine.IsMoveLegal(state, "e7", "e6"));
    }

    [Fact]
    public void BlackPawn_ForwardTwo_Legal()
    {
        var state = FromFen("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq - 0 1");
        Assert.True(_engine.IsMoveLegal(state, "e7", "e5"));
    }

    // --- En Passant ---

    [Fact]
    public void Pawn_EnPassant_Legal()
    {
        var state = FromFen("rnbqkbnr/pppp1ppp/8/4pP2/8/8/PPPPP1PP/RNBQKBNR w KQkq e6 0 1");
        Assert.True(_engine.IsMoveLegal(state, "f5", "e6"));
    }

    [Fact]
    public void Pawn_EnPassant_WrongSquare_Illegal()
    {
        var state = FromFen("rnbqkbnr/pppp1ppp/8/4pP2/8/8/PPPPP1PP/RNBQKBNR w KQkq e6 0 1");
        Assert.False(_engine.IsMoveLegal(state, "f5", "g6"));
    }

    // --- Knight ---

    [Fact]
    public void Knight_LShape_Legal()
    {
        var state = StartingPosition();
        Assert.True(_engine.IsMoveLegal(state, "b1", "c3"));
        Assert.True(_engine.IsMoveLegal(state, "b1", "a3"));
    }

    [Fact]
    public void Knight_Straight_Illegal()
    {
        var state = StartingPosition();
        Assert.False(_engine.IsMoveLegal(state, "b1", "b3"));
    }

    [Fact]
    public void Knight_JumpsOverPieces()
    {
        var state = StartingPosition();
        // Knight can jump over pawns on rank 2
        Assert.True(_engine.IsMoveLegal(state, "b1", "c3"));
    }

    // --- Bishop ---

    [Fact]
    public void Bishop_Diagonal_Legal()
    {
        var state = FromFen("rnbqkbnr/pppppppp/8/8/8/4P3/PPPP1PPP/RNBQKBNR w KQkq - 0 1");
        Assert.True(_engine.IsMoveLegal(state, "f1", "e2"));
    }

    [Fact]
    public void Bishop_Straight_Illegal()
    {
        var state = FromFen("rnbqk1nr/pppppppp/8/8/2b5/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1");
        Assert.False(_engine.IsMoveLegal(state, "c4", "c5"));
    }

    [Fact]
    public void Bishop_Blocked_Illegal()
    {
        var state = StartingPosition();
        // Bishop on f1 blocked by pawn on e2
        Assert.False(_engine.IsMoveLegal(state, "f1", "d3"));
    }

    // --- Rook ---

    [Fact]
    public void Rook_Horizontal_Legal()
    {
        var state = FromFen("rnbqkbnr/pppppppp/8/8/8/P7/1PPPPPPP/RNBQKBNR w KQkq - 0 1");
        Assert.True(_engine.IsMoveLegal(state, "a1", "a2"));
    }

    [Fact]
    public void Rook_Diagonal_Illegal()
    {
        var state = FromFen("8/8/8/8/8/8/8/R3K2R w KQ - 0 1");
        Assert.False(_engine.IsMoveLegal(state, "a1", "b2"));
    }

    [Fact]
    public void Rook_Blocked_Illegal()
    {
        var state = StartingPosition();
        // Rook on a1 blocked by pawn on a2
        Assert.False(_engine.IsMoveLegal(state, "a1", "a3"));
    }

    // --- Queen ---

    [Fact]
    public void Queen_Diagonal_Legal()
    {
        var state = FromFen("rnbqkbnr/pppppppp/8/8/8/3P4/PPP1PPPP/RNBQKBNR w KQkq - 0 1");
        Assert.True(_engine.IsMoveLegal(state, "d1", "d2"));
    }

    [Fact]
    public void Queen_Straight_Legal()
    {
        var state = FromFen("rnbqkbnr/pppppppp/8/8/8/3P4/PPP1PPPP/RNBQKBNR w KQkq - 0 1");
        Assert.True(_engine.IsMoveLegal(state, "d1", "d2"));
    }

    // --- King ---

    [Fact]
    public void King_OneSquare_Legal()
    {
        var state = FromFen("rnbqkbnr/pppppppp/8/8/8/4P3/PPPP1PPP/RNBQKBNR w KQkq - 0 1");
        Assert.True(_engine.IsMoveLegal(state, "e1", "e2"));
    }

    [Fact]
    public void King_TwoSquaresNonCastling_Illegal()
    {
        // King on e1, no castling scenario (no rook)
        var state = FromFen("8/8/8/8/8/8/8/4K3 w - - 0 1");
        Assert.False(_engine.IsMoveLegal(state, "e1", "g1"));
    }

    [Fact]
    public void King_CaptureOwnPiece_Illegal()
    {
        var state = FromFen("rnbqkbnr/pppppppp/8/8/8/4P3/PPPP1PPP/RNBQKBNR w KQkq - 0 1");
        Assert.False(_engine.IsMoveLegal(state, "e1", "d1"));
    }

    // --- Castling ---

    [Fact]
    public void Castling_KingSide_White_Legal()
    {
        var state = FromFen("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1");
        Assert.True(_engine.IsMoveLegal(state, "e1", "g1"));
    }

    [Fact]
    public void Castling_QueenSide_White_Legal()
    {
        var state = FromFen("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1");
        Assert.True(_engine.IsMoveLegal(state, "e1", "c1"));
    }

    [Fact]
    public void Castling_KingSide_Black_Legal()
    {
        var state = FromFen("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R b KQkq - 0 1");
        Assert.True(_engine.IsMoveLegal(state, "e8", "g8"));
    }

    [Fact]
    public void Castling_NoRight_Illegal()
    {
        var state = FromFen("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w - - 0 1");
        Assert.False(_engine.IsMoveLegal(state, "e1", "g1"));
    }

    [Fact]
    public void Castling_PathBlocked_Illegal()
    {
        var state = FromFen("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R2QK2R w KQkq - 0 1");
        Assert.False(_engine.IsMoveLegal(state, "e1", "c1"));
    }

    [Fact]
    public void Castling_KingInCheck_Illegal()
    {
        // Black rook attacks e1 (e-file is open)
        var state = FromFen("4k3/8/8/8/4r3/8/PPPP1PPP/R3K2R w KQ - 0 1");
        Assert.False(_engine.IsMoveLegal(state, "e1", "g1"));
    }

    [Fact]
    public void Castling_KingPassesThroughAttack_Illegal()
    {
        // Black rook attacks f1
        var state = FromFen("4k3/8/8/8/5r2/8/PPPPP1PP/R3K2R w KQ - 0 1");
        Assert.False(_engine.IsMoveLegal(state, "e1", "g1"));
    }

    // --- IsInCheck ---

    [Fact]
    public void IsInCheck_True_WhenKingAttacked()
    {
        var state = FromFen("rnbqkbnr/ppppp2p/5p2/6pQ/4P3/8/PPPP1PPP/RNB1KBNR b KQkq - 0 1");
        Assert.True(_engine.IsInCheck(state, PiecesColors.Black));
    }

    [Fact]
    public void IsInCheck_False_StartingPosition()
    {
        var state = StartingPosition();
        Assert.False(_engine.IsInCheck(state, PiecesColors.White));
        Assert.False(_engine.IsInCheck(state, PiecesColors.Black));
    }

    // --- Can't move into check ---

    [Fact]
    public void King_MoveIntoCheck_Illegal()
    {
        // King on e1, black rook on d8 — king can't go to d1 or d2
        var state = FromFen("3rk3/8/8/8/8/8/8/4K3 w - - 0 1");
        Assert.False(_engine.IsMoveLegal(state, "e1", "d1"));
        Assert.False(_engine.IsMoveLegal(state, "e1", "d2"));
    }

    [Fact]
    public void PinnedPiece_CantMoveAwayFromPin()
    {
        // White bishop on d2 is pinned by black rook on a5 through e1 king? No, that's not a pin.
        // Better: White rook on e2, black rook on e8, white king on e1 — rook is pinned on e-file
        var state = FromFen("4r1k1/8/8/8/8/8/4R3/4K3 w - - 0 1");
        // Rook on e2 pinned along e-file by black rook e8
        Assert.False(_engine.IsMoveLegal(state, "e2", "d2")); // moving off e-file exposes king
        Assert.True(_engine.IsMoveLegal(state, "e2", "e3"));  // moving along pin is ok
    }

    // --- Checkmate ---

    [Fact]
    public void IsCheckmate_FoolsMate()
    {
        // After 1.f3 e5 2.g4 Qh4#
        var state = FromFen("rnb1kbnr/pppp1ppp/8/4p3/6Pq/5P2/PPPPP2P/RNBQKBNR w KQkq - 0 1");
        Assert.True(_engine.IsCheckmate(state, PiecesColors.White));
    }

    [Fact]
    public void IsCheckmate_ScholarsMate()
    {
        // Scholar's mate position: Qxf7#
        var state = FromFen("r1bqkb1r/pppp1Qpp/2n2n2/4p3/2B1P3/8/PPPP1PPP/RNB1K1NR b KQkq - 0 1");
        Assert.True(_engine.IsCheckmate(state, PiecesColors.Black));
    }

    [Fact]
    public void IsCheckmate_False_WhenNotInCheck()
    {
        var state = StartingPosition();
        Assert.False(_engine.IsCheckmate(state, PiecesColors.White));
    }

    // --- Stalemate ---

    [Fact]
    public void IsStalemate_KingTrapped()
    {
        // Black king on a8, white queen on b6, white king on c8 — stalemate for black
        // Actually let me use a classic stalemate: Ka1, white Qb3, Kc2 — stalemate for black with king on a1
        var state = FromFen("8/8/8/8/8/1Q6/2K5/k7 b - - 0 1");
        Assert.True(_engine.IsStalemate(state, PiecesColors.Black));
    }

    [Fact]
    public void IsStalemate_False_WhenMovesAvailable()
    {
        var state = StartingPosition();
        Assert.False(_engine.IsStalemate(state, PiecesColors.White));
    }

    // --- GetLegalMoves ---

    [Fact]
    public void GetLegalMoves_StartingPosition_Has20Moves()
    {
        var state = StartingPosition();
        var moves = _engine.GetLegalMoves(state, PiecesColors.White);
        // 16 pawn moves + 4 knight moves = 20
        Assert.Equal(20, moves.Count);
    }

    [Fact]
    public void GetLegalMoves_CheckmatePosition_HasZeroMoves()
    {
        var state = FromFen("rnb1kbnr/pppp1ppp/8/4p3/6Pq/5P2/PPPPP2P/RNBQKBNR w KQkq - 0 1");
        var moves = _engine.GetLegalMoves(state, PiecesColors.White);
        Assert.Empty(moves);
    }

    [Fact]
    public void GetLegalMoves_IncludesCastling()
    {
        var state = FromFen("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1");
        var moves = _engine.GetLegalMoves(state, PiecesColors.White);
        Assert.Contains(moves, m => m.From == "e1" && m.To == "g1");
        Assert.Contains(moves, m => m.From == "e1" && m.To == "c1");
    }

    [Fact]
    public void GetLegalMoves_IncludesEnPassant()
    {
        var state = FromFen("rnbqkbnr/pppp1ppp/8/4pP2/8/8/PPPPP1PP/RNBQKBNR w KQkq e6 0 1");
        var moves = _engine.GetLegalMoves(state, PiecesColors.White);
        Assert.Contains(moves, m => m.From == "f5" && m.To == "e6");
    }

    // --- General ---

    [Fact]
    public void CantCaptureOwnPiece()
    {
        var state = StartingPosition();
        Assert.False(_engine.IsMoveLegal(state, "a1", "a2")); // rook can't take own pawn
    }

    [Fact]
    public void CanCaptureEnemyPiece()
    {
        // White knight on c3 can capture black pawn on e4
        var state = FromFen("rnbqkbnr/pppp1ppp/8/8/4p3/2N5/PPPPPPPP/R1BQKBNR w KQkq - 0 1");
        Assert.True(_engine.IsMoveLegal(state, "c3", "e4"));
    }

    [Fact]
    public void Rook_CaptureEnemy_Legal()
    {
        // White rook on h1 captures black rook on h7
        var state = FromFen("4k3/7r/8/8/8/8/8/4K2R w K - 0 1");
        Assert.True(_engine.IsMoveLegal(state, "h1", "h7"));
    }
}
