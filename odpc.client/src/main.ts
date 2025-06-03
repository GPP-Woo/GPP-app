import "./assets/base.css";
import "./assets/design-tokens.scss";
import "./assets/main.scss";

import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import guards from "./router/guards.ts";
import formInvalidHandler from "./directives/form-invalid-handler";
import { getLijsten } from "./stores/lijsten";

const app = createApp(App);

app.directive("form-invalid-handler", formInvalidHandler);

app.use(guards, router);
app.use(router);

app.mount("#app");
