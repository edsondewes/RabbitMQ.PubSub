import { Component, html } from "https://unpkg.com/htm@2.0.0/preact/standalone.mjs";
import { notify } from './Notification.js';

const HubEvent = 'ReceiveBackgroundJobReport';

export default class JobProgress extends Component {
  constructor(props) {
    super(props);

    this.receiveReport = this.receiveReport.bind(this);
    this.scheduleJob = this.scheduleJob.bind(this);
  }

  componentDidMount() {
    this.props.hubContext.connection.on(HubEvent, this.receiveReport);
  }

  componentWillUnmount() {
    this.props.hubContext.connection.off(HubEvent, this.receiveReport);
  }

  receiveReport({ count, current }) {
    this.setState({ count, current });
  }

  scheduleJob() {
    fetch('http://localhost:5000/api/jobs', {
      method: 'POST',
      headers: {
        "report-id": this.props.hubContext.reportId
      }
    });

    notify("Check out the progress bar!");
  }
  
  render({ }, { count = 10, current = 0 }) {
    return html`
      <div>
        <button onClick=${this.scheduleJob}>Schedule a background job</button>
        <progress class="job-progress" value=${current} max="${count}"></progress>
      </div>
    `;
  }
}
