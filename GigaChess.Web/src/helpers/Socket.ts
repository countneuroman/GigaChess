export default class Sockets {
    constructor(
        readonly baseUrl: string,
        readonly path: string
    ) {}

    public connect = () => {
        const ws = new WebSocket(this.baseUrl + this.path)

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
}