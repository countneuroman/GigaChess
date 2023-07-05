import Socket from './Socket'

let current: Socket | undefined
const socketWorker: Worker = self as any

socketWorker.onmessage = (msg: MessageEvent) => {
    switch (msg.data.topic) {
        case 'connect':
            if (current) current.connect()
            break
        case 'disconnect':
            if (current) current.disconnect()
            break
        case 'send':
            if (current) current.send(msg.data)
            break
        case 'ask':
            break
    }
}