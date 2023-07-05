export default class Sockets {
    ws: WebSocket | undefined
    constructor(
        readonly baseUrl: string,
        readonly path: string
    ) {}

    public connect = () => {
        const ws = this.ws = new WebSocket(this.baseUrl + this.path)

        try {
            ws.onopen = () => {
                console.log('Connection opened')
            }

            ws.onclose = () => {
                console.log('Connection closed')
            }

            ws.onmessage = e => {
                const message = JSON.parse(e.data)
            }

            ws.onerror = e => {
                console.log(e)
            }


        } catch (error) {
            console.log(error)
        }
    }

    public disconnect = () => {
        const ws = this.ws
        if (ws) {
            ws.close()
            console.log('Connection closed')
        }
    }

    public send(data: any): void {
        const message = JSON.stringify(data)
        this.ws!.send(message)
    }
}