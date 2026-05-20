<template>
  <div class="app">
    <header>
      <h1>EzDns 管理界面</h1>
    </header>
    <nav>
      <router-link to="/rules">DNS 规则管理</router-link>
    </nav>
    <router-view />
  </div>
</template>

<style scoped>
.app {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}
nav {
  margin: 20px 0;
}
nav a {
  margin-right: 20px;
}
</style>