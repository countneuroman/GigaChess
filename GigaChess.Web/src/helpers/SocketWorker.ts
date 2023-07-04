import Socket from './Socket'

let current: Socket | undefined
const socketWorker: Worker = self as any

socketWorker.onmessage = (msg: MessageEvent) => {
    switch (msg.data.topic)
}