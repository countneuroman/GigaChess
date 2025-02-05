import React, { useEffect, useRef, useState } from 'react';
import ReactDOM from 'react-dom/client'
import { Chessground as ChessgroundApi } from 'chessground';

import { Api } from 'chessground/api';
import { Config } from 'chessground/config';

interface Props {
  width?: number
  height?: number
  contained?: boolean;
  config?: Config
}

function Chessground({
  width = 900, height = 900, config = {}, contained = false,
}: Props) {
  const [api, setApi] = useState<Api | null>(null);

  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (ref && ref.current && !api) {
      const chessgroundApi = ChessgroundApi(ref.current, {
        animation: { enabled: true, duration: 200 },
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

const App: React.FC = () =>  {
    return (
      <div>
        <Chessground width={600} height={600} contained={false} />
      </div>
    );
  };

export default App;

const root = ReactDOM.createRoot(document.getElementById('root') as HTMLElement);
root.render(<App />);