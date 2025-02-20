using GigaChess.Api.Models;

namespace GigaChess.Api.Services;

public class ChessService
{
    private readonly ILogger<ChessService> _logger;
    private readonly char[][] _board;
    private readonly string[][] _boardPositions;
    private readonly Dictionary<string, int[]> _positionMap;
    public ChessService(ILogger<ChessService> logger)
    {
        _logger = logger;
        _positionMap = new Dictionary<string, int[]>()
        {
            { "a8", [0, 0] },
            { "b8", [0, 1] },
            { "c8", [0, 2] },
            { "d8", [0, 3] },
            { "e8", [0, 4] },
            { "f8", [0, 5] },
            { "g8", [0, 6] },
            { "h8", [0, 7] },
            { "a7", [1, 0] },
            { "b7", [1, 1] },
            { "c7", [1, 2] },
            { "d7", [1, 3] },
            { "e7", [1, 4] },
            { "f7", [1, 5] },
            { "g7", [1, 6] },
            { "h7", [1, 7] },
            { "a6", [2, 0] },
            { "b6", [2, 1] },
            { "c6", [2, 2] },
            { "d6", [2, 3] },
            { "e6", [2, 4] },
            { "f6", [2, 5] },
            { "g6", [2, 6] },
            { "h6", [2, 7] },
            { "a5", [3, 0] },
            { "b5", [3, 1] },
            { "c5", [3, 2] },
            { "d5", [3, 3] },
            { "e5", [3, 4] },
            { "f5", [3, 5] },
            { "g5", [3, 6] },
            { "h5", [3, 7] },
            { "a4", [4, 0] },
            { "b4", [4, 1] },
            { "c4", [4, 2] },
            { "d4", [4, 3] },
            { "e4", [4, 4] },
            { "f4", [4, 5] },
            { "g4", [4, 6] },
            { "h4", [4, 7] },
            { "a3", [5, 0] },
            { "b3", [5, 1] },
            { "c3", [5, 2] },
            { "d3", [5, 3] },
            { "e3", [5, 4] },
            { "f3", [5, 5] },
            { "g3", [5, 6] },
            { "h3", [5, 7] },
            { "a2", [6, 0] },
            { "b2", [6, 1] },
            { "c2", [6, 2] },
            { "d2", [6, 3] },
            { "e2", [6, 4] },
            { "f2", [6, 5] },
            { "g2", [6, 6] },
            { "h2", [6, 7] },
            { "a1", [7, 0] },
            { "b1", [7, 1] },
            { "c1", [7, 2] },
            { "d1", [7, 3] },
            { "e1", [7, 4] },
            { "f1", [7, 5] },
            { "g1", [7, 6] },
            { "h1", [7, 7] }
        };
        _boardPositions = new string[][]
        {
            ["a8", "b8", "c8", "d8", "e8", "f8", "g8", "h8"],
            ["a7", "b7", "c7", "d7", "e7", "f7", "g7", "h7"],
            ["a6", "b6", "c6", "d6", "e6", "f6", "g6", "h6"],
            ["a5", "b5", "c5", "d5", "e5", "f5", "g5", "h5"],
            ["a4", "b4", "c4", "d4", "e4", "f4", "g4", "h4"],
            ["a3", "b3", "c3", "d3", "e3", "f3", "g3", "h3"],
            ["a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2"],
            ["a1", "b1", "c1", "d1", "e1", "f1", "g1", "h1"],
        };
        _board = new char[][]
        {
            [ 'r', 'n', 'b', 'q', 'k', 'b', 'n', 'r' ],
            [ 'p', 'p', 'p', 'p', 'p', 'p', 'p', 'p' ],
            [ ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' ],
            [ ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' ],
            [ ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' ],
            [ ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' ],
            [ 'P', 'P', 'P', 'P', 'P', 'P', 'P', 'P' ],
            [ 'R', 'N', 'B', 'Q', 'K', 'B', 'N', 'R' ]
        };
    }
    
    public async Task<string[]> GetLogalMovies(Piece piece)
    {
        var legalMoves = Array.Empty<string>();
        switch (piece.Role)
        {
            case PiecesRoles.Pawn:
                var pieceMatrixPos = _positionMap[piece.Position];
                if (piece.Color == PiecesColors.White)
                {
                    if (pieceMatrixPos[1] < 7 && _board[pieceMatrixPos[0] - 1][pieceMatrixPos[1]] == ' ')
                    {
                        if (pieceMatrixPos[0] == 6)
                        {
                            legalMoves =
                            [
                                $"{_boardPositions[pieceMatrixPos[0] - 1][pieceMatrixPos[1]]}",
                                $"{_boardPositions[pieceMatrixPos[0] - 2][pieceMatrixPos[1]]}"
                            ];
                        }
                        else
                        {
                            legalMoves =
                            [
                                $"{_boardPositions[pieceMatrixPos[0] - 1][pieceMatrixPos[1]]}",
                            ];
                        }
                    }
                }
                if (piece.Color == PiecesColors.Black)
                {
                    if (pieceMatrixPos[1] > 0 && _board[pieceMatrixPos[0] + 1][pieceMatrixPos[1]] == ' ')
                    {
                        if (pieceMatrixPos[0] == 1)
                        {
                            legalMoves =
                            [
                                $"{_boardPositions[pieceMatrixPos[0] + 1][pieceMatrixPos[1]]}",
                                $"{_boardPositions[pieceMatrixPos[0] + 2][pieceMatrixPos[1]]}"
                            ];
                        }
                        else
                        {
                            legalMoves =
                            [
                                $"{_boardPositions[pieceMatrixPos[0] + 1][pieceMatrixPos[1]]}",
                            ];
                        }
                    }
                }
                break;
            case PiecesRoles.Knight:
                break;
            case PiecesRoles.Bishop:
                break;
            case PiecesRoles.Rook:
                break;
            case PiecesRoles.Queen:
                break;
            case PiecesRoles.King:
                break;
        }

        return legalMoves;
    }
}