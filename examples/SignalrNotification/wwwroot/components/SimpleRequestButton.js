import { Component, html } from "https://unpkg.com/htm@2.0.0/preact/standalone.mjs";
import { notify } from './Notification.js';

const ActionReportEvent = "ReceiveActionReport";

export default class SimpleRequestButton extends Component {
  constructor(props) {
    super(props);

    this.submitRequest = this.submitRequest.bind(this);
    this.receiveActionReport = this.receiveActionReport.bind(this);
  }

  componentDidMount() {
    this.props.hubContext.connection.on(ActionReportEvent, this.receiveActionReport);
  }

  componentWillUnmount() {
    this.props.hubContext.connection.off(ActionReportEvent, this.receiveActionReport);
  }

  submitRequest() {
    fetch('http://localhost:5000/api/jobs', {
      headers: {
        "report-id": this.props.hubContext.reportId
      }
    });
  }

  receiveActionReport({ text }) {
    notify(text);
  }
  
  render() {
    return html`
      <button onClick=${this.submitRequest}>Submit a request</button>
    `;
  }
}
