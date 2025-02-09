import React, { useEffect, useRef, useState, createContext, useContext } from 'react';
import ReactDOM from 'react-dom/client'
import { Chessground as ChessgroundApi } from 'chessground';

import { Api } from 'chessground/api';
import { Config } from 'chessground/config';    

import "chessground/assets/chessground.base.css";
import "chessground/assets/chessground.brown.css";
import "chessground/assets/chessground.cburnett.css";
import {Piece} from "chessground/src/types";

const ChessgroundContext = createContext<{
    api: Api | null;
    setApi: React.Dispatch<React.SetStateAction<Api | null>>;
}>({
    api: null,
    setApi: () => {},
});


const sendPiecePosition = async(piece: Piece, pos: string) => {
    console.log("Фигура: " + piece.color + " " + piece.role + " на клетке " + pos);
    //TODO: передавать адрес бэка не хардкодом
    const response = await fetch('http://localhost:7130/api/Chess/GetPiecePostition', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({color: piece.color, role: piece.role, position: pos})
    });
    console.log("Статутс ответа: " + response.status)
    if (response.status === 200) {
        console.log("Success");
    } else {
        console.log("Error: " + response.status);
    }
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
  const { api, setApi } = useContext(ChessgroundContext);

  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (ref && ref.current && !api) {
      const chessgroundApi = ChessgroundApi(ref.current, {
        animation: { enabled: true, duration: 200 },
        events: {
            select: (pos) => {
                const piece = chessgroundApi.state.pieces.get(pos);
                if (piece) {
                    sendPiecePosition(piece, pos);
                } else {
                    console.log(`Выбрана пустая клетка: ${pos}`);
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
    const { api } = useContext(ChessgroundContext);
    
    const handleReset = () => {
        if (api) {
            api.set({
                fen: 'start',
            });
        }
    };

    return (
        <button onClick={handleReset}>Reset Position</button>
    );
}

const App: React.FC = () => {
    const [api, setApi] = useState<Api | null>(null);
    return (
        <ChessgroundContext.Provider value={{ api, setApi }}>
            <div>
            <Chessground width={900} height={900} contained={false}/>
            <ResetBoard />
            </div>
        </ChessgroundContext.Provider>
    );
};

const root = ReactDOM.createRoot(document.getElementById('board') as HTMLElement);
root.render(<App />);