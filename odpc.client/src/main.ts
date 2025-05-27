import "./assets/base.css";
import "./assets/design-tokens.scss";
import "./assets/main.scss";

import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import formInvalidHandler from "./directives/form-invalid-handler";
import { getLijsten } from "./stores/lijsten";

const app = createApp(App);

app.use(router);
app.directive("form-invalid-handler", formInvalidHandler);

(async () => {
  try {
    // Loader ...
    // Preload waardelijsten to be used in different app components
    app.provide("lijsten", await getLijsten());
    app.mount("#app");
  } catch {
    // Error ...
  }
})();
