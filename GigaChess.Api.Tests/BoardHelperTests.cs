using GigaChess.Api.Helpers;
using GigaChess.Api.Models;

namespace GigaChess.Api.Tests;

public class BoardHelperTests
{
    [Fact]
    public void InitStartingPosition_ReturnsCorrectBoard()
    {
        var board = BoardHelper.InitStartingPosition();

        // Black back rank
        Assert.Equal("bR", board[0, 0]);
        Assert.Equal("bN", board[0, 1]);
        Assert.Equal("bB", board[0, 2]);
        Assert.Equal("bQ", board[0, 3]);
        Assert.Equal("bK", board[0, 4]);
        Assert.Equal("bR", board[0, 7]);

        // Black pawns
        for (int c = 0; c < 8; c++)
            Assert.Equal("bP", board[1, c]);

        // Empty middle
        for (int r = 2; r <= 5; r++)
        for (int c = 0; c < 8; c++)
            Assert.Null(board[r, c]);

        // White pawns
        for (int c = 0; c < 8; c++)
            Assert.Equal("wP", board[6, c]);

        // White back rank
        Assert.Equal("wR", board[7, 0]);
        Assert.Equal("wN", board[7, 1]);
        Assert.Equal("wB", board[7, 2]);
        Assert.Equal("wQ", board[7, 3]);
        Assert.Equal("wK", board[7, 4]);
        Assert.Equal("wR", board[7, 7]);
    }

    [Fact]
    public void ToFen_StartingPosition_ReturnsCorrectFen()
    {
        var state = new GameState
        {
            GameId = "test",
            Board = BoardHelper.InitStartingPosition()
        };

        var fen = BoardHelper.ToFen(state);

        Assert.Equal("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", fen);
    }

    [Fact]
    public void FromFen_StartingPosition_ParsesCorrectly()
    {
        var state = new GameState { GameId = "test" };
        BoardHelper.FromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", state);

        Assert.Equal(PiecesColors.White, state.ActiveColor);
        Assert.Equal(CastlingRights.All, state.CastlingRights);
        Assert.Null(state.EnPassantSquare);
        Assert.Equal(0, state.HalfmoveClock);
        Assert.Equal(1, state.FullmoveNumber);
        Assert.Equal("wK", state.Board[7, 4]);
        Assert.Equal("bK", state.Board[0, 4]);
    }

    [Fact]
    public void FromFen_AfterE4_ParsesEnPassantAndColor()
    {
        var state = new GameState { GameId = "test" };
        BoardHelper.FromFen("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1", state);

        Assert.Equal(PiecesColors.Black, state.ActiveColor);
        Assert.Equal("e3", state.EnPassantSquare);
        Assert.Equal("wP", state.Board[4, 4]);
        Assert.Null(state.Board[6, 4]); // e2 now empty
    }

    [Fact]
    public void ToFen_And_FromFen_Roundtrip()
    {
        var original = new GameState
        {
            GameId = "test",
            Board = BoardHelper.InitStartingPosition()
        };

        var fen = BoardHelper.ToFen(original);

        var restored = new GameState { GameId = "test2" };
        BoardHelper.FromFen(fen, restored);

        var fenAgain = BoardHelper.ToFen(restored);
        Assert.Equal(fen, fenAgain);
    }

    [Theory]
    [InlineData("a1", 7, 0)]
    [InlineData("h8", 0, 7)]
    [InlineData("e2", 6, 4)]
    [InlineData("d5", 3, 3)]
    public void SquareToIndices_ConvertsCorrectly(string square, int expectedRow, int expectedCol)
    {
        var (row, col) = BoardHelper.SquareToIndices(square);
        Assert.Equal(expectedRow, row);
        Assert.Equal(expectedCol, col);
    }

    [Theory]
    [InlineData(7, 0, "a1")]
    [InlineData(0, 7, "h8")]
    [InlineData(6, 4, "e2")]
    public void IndicesToSquare_ConvertsCorrectly(int row, int col, string expected)
    {
        Assert.Equal(expected, BoardHelper.IndicesToSquare(row, col));
    }

    [Fact]
    public void SquareToIndices_InvalidSquare_Throws()
    {
        Assert.Throws<ArgumentException>(() => BoardHelper.SquareToIndices("z9"));
    }

    [Fact]
    public void FromFen_InvalidPartCount_Throws()
    {
        var state = new GameState { GameId = "test" };
        Assert.Throws<ArgumentException>(() => BoardHelper.FromFen("bad fen", state));
    }

    [Fact]
    public void FromFen_CastlingNone_ParsesCorrectly()
    {
        var state = new GameState { GameId = "test" };
        BoardHelper.FromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w - - 0 1", state);

        Assert.Equal(CastlingRights.None, state.CastlingRights);
    }
}
