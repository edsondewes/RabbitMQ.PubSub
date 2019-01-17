import { HubConnectionBuilder } from "https://unpkg.com/@aspnet/signalr@1.1.0/dist/esm/index.js";
import { Component, html } from "https://unpkg.com/htm@2.0.0/preact/standalone.mjs";
import BackgroundJobProgress from './BackgroundJobProgress.js';
import SimpleRequestButton from './SimpleRequestButton.js';
import { Notification } from './Notification.js';

export default class App extends Component {
  async componentDidMount() {
    const hubConnection = new HubConnectionBuilder()
      .withUrl("/notification")
      .build();

    await hubConnection.start();
    const reportId = await hubConnection.invoke('CreateRequestId');

    this.setState({
      hubContext: {
        connection: hubConnection,
        reportId: reportId
      }
    });
  }

  componentWillUnmount() {
    if (this.state.hubContext) {
      this.state.hubContext.connection.stop();
    }
  }

  render({ }, { hubContext }) {
    return html`
      <div>
        <h1>Notifications Page</h1>
        <${Notification} />
        ${!hubContext
          ? html`<p>Connecting to the server...</p>`
          : html`
            <div>
              <${SimpleRequestButton} hubContext=${hubContext} />
              <hr />
              <${BackgroundJobProgress} hubContext=${hubContext} />
            </div>`}
      </div>
    `;
  }
}
