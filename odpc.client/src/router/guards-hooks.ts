import type { App } from "vue";
import type { Router, RouteLocationNormalized } from "vue-router";
import { useAppData } from "@/composables/use-app-data";
import { setPreviousRoute } from "@/composables/use-previous-route";

async function authGuard(to: RouteLocationNormalized) {
  const requiresAuth = to.matched.some((route) => route.meta.requiresAuth);
  const requiresAdmin = to.matched.some((route) => route.meta.requiresAdmin);

  const { fetchData, user } = useAppData();

  await fetchData();

  if ((requiresAuth || requiresAdmin) && !user.value?.isLoggedIn) {
    return { name: "login" };
  }

  if (requiresAdmin && !user.value?.isAdmin) {
    return { name: "forbidden" };
  }
}

function titleHook(to: RouteLocationNormalized) {
  document.title = `${to.meta?.title || ""} | ${import.meta.env.VITE_APP_TITLE}`;
}

function previousRouteHook(_: RouteLocationNormalized, from: RouteLocationNormalized) {
  setPreviousRoute(from);
}

export default {
  install(_: App, router: Router) {
    router.beforeEach(authGuard);
    router.beforeEach(titleHook);
    router.beforeEach(previousRouteHook);
  }
};
