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
    // Preload waardelijsten to be used in different app components
    app.provide("lijsten", await getLijsten());
    app.mount("#app");
  } catch {
    const loadingMessage = document.getElementById("loadingMessage");
    
    if (loadingMessage)
      loadingMessage.innerText = "Er is iets fout gegaan. De applicatie kan niet worden geladen.";
  }
})();
