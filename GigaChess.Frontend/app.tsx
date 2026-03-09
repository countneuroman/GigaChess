import React, { useEffect, useRef, useState, createContext, useContext } from 'react';
import ReactDOM from 'react-dom/client'
import { Chessground as ChessgroundApi } from '@lichess-org/chessground';

import { Api } from '@lichess-org/chessground/dist/api';
import { Config } from '@lichess-org/chessground/dist/config';

import "@lichess-org/chessground/assets/chessground.base.css";
import "@lichess-org/chessground/assets/chessground.brown.css";
import "@lichess-org/chessground/assets/chessground.cburnett.css";

const API_BASE = 'http://localhost:7130';

const ChessgroundContext = createContext<{
    api: Api | null;
    setApi: React.Dispatch<React.SetStateAction<Api | null>>;
    gameId: string | null;
    setGameId: React.Dispatch<React.SetStateAction<string | null>>;
}>({
    api: null,
    setApi: () => {},
    gameId: null,
    setGameId: () => {},
});

const createGame = async (): Promise<{ gameId: string; fen: string } | null> => {
    const response = await fetch(`${API_BASE}/api/Game/New`, { method: 'POST' });
    if (response.ok) {
        const data = await response.json();
        return { gameId: data.gameId, fen: data.fen };
    }
    return null;
};

const getLegalMoves = async (gameId: string, square: string): Promise<string[]> => {
    const response = await fetch(`${API_BASE}/api/Game/LegalMoves`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ gameId, square }),
    });
    if (response.ok) {
        const moves: { from: string; to: string }[] = await response.json();
        return moves.map(m => m.to);
    }
    return [];
};

interface Props {
  width?: number
  height?: number
  contained?: boolean;
  config?: Config
}

function Chessground({
  width = 900,
  height = 900, 
  config = {
  },
  contained = false,
}: Props) {
  const { api, setApi, gameId } = useContext(ChessgroundContext);

  const gameIdRef = useRef(gameId);
  useEffect(() => { gameIdRef.current = gameId; }, [gameId]);

  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (ref && ref.current && !api) {
      const chessgroundApi = ChessgroundApi(ref.current, {
        animation: { enabled: true, duration: 200 },
        events: {
            select: async (pos) => {
                const currentGameId = gameIdRef.current;
                if (!currentGameId) return;
                const piece = chessgroundApi.state.pieces.get(pos);
                if (piece) {
                    const moves = await getLegalMoves(currentGameId, pos);
                    chessgroundApi.set({
                        movable: {
                            free: false,
                            dests: new Map([[pos, moves]]),
                    }});
                }
            }
        },
        ...config,
      });
      setApi(chessgroundApi);
    } else if (ref && ref.current && api) {
      api.set(config);
    }
  }, [ref]);

  useEffect(() => {
    api?.set(config);
  }, [api, config]);
  
  
  
  return (
        <div style={{ height: contained ? '100%' : height, width: contained ? '100%' : width }}>
            <div ref={ref} style={{ height: '100%', width: '100%', display: 'table' }} />
        </div>
  );
}

const ResetBoard = () => {
    const { api, setGameId } = useContext(ChessgroundContext);

    const handleReset = async () => {
        const game = await createGame();
        if (game && api) {
            setGameId(game.gameId);
            api.set({ fen: game.fen });
        }
    };

    return (
        <button onClick={handleReset}>Reset Position</button>
    );
}

const App: React.FC = () => {
    const [api, setApi] = useState<Api | null>(null);
    const [gameId, setGameId] = useState<string | null>(null);

    useEffect(() => {
        if (!api) return;
        createGame().then(game => {
            if (game) {
                setGameId(game.gameId);
                api.set({ fen: game.fen });
            }
        });
    }, [api]);

    return (
        <ChessgroundContext.Provider value={{ api, setApi, gameId, setGameId }}>
            <div>
            <Chessground width={900} height={900} contained={false}/>
            <ResetBoard />
            </div>
        </ChessgroundContext.Provider>
    );
};

const root = ReactDOM.createRoot(document.getElementById('board') as HTMLElement);
root.render(<App />);