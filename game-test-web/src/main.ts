import './styles.css';
import { BrowserGameApp } from './game/app';

const appRoot = document.querySelector<HTMLDivElement>('#app');

if (!appRoot) {
  throw new Error('Missing #app host element.');
}

new BrowserGameApp(appRoot);
