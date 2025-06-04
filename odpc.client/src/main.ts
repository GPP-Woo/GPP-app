import "./assets/base.css";
import "./assets/design-tokens.scss";
import "./assets/main.scss";

import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import guardsHooks from "./router/guards-hooks.ts";
import formInvalidHandler from "./directives/form-invalid-handler";

const app = createApp(App);

app.directive("form-invalid-handler", formInvalidHandler);

app.use(guardsHooks, router);
app.use(router);

app.mount("#app");
