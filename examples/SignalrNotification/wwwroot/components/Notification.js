import { Component, html } from "https://unpkg.com/htm@2.0.0/preact/standalone.mjs";

const EventName = 'app-notify';

export function notify(text) {
  const event = new CustomEvent(EventName, { detail: text });
  document.dispatchEvent(event);
}

export class Notification extends Component {
  constructor(props) {
    super(props);
    this.receiveNotification = this.receiveNotification.bind(this);

    this.state = {
      items: []
    };
  }

  componentDidMount() {
    document.addEventListener(EventName, this.receiveNotification);
  }

  componentWillUnmount() {
    document.removeEventListener(EventName, this.receiveNotification);
  }

  receiveNotification({ detail }) {
    this.setState(prev => ({
      items: [
        ...prev.items,
        detail
      ]
    }));
  }

  remove(removeIndex) {
    this.setState(prev => ({
      items: prev.items.filter((_, itemIndex) => itemIndex !== removeIndex)
    }));
  }

  render({ }, { items }) {
    return html`
      <ul class="notification-list">
        ${items.map((item, index) => html`<li onClick=${() => this.remove(index)}>${item}</li>`)}
      </ul>
    `;
  }
}
