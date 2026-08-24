<script setup>
import ContourPanel from './components/ContourPanel.vue'
import './style.css'
</script>

<template>
  <div class="app-container">
    <header class="app-header">
      <h1>阻抗等高線分析</h1>
    </header>
    <main class="app-main">
      <ContourPanel />
    </main>
  </div>
</template>

<style scoped>
.app-container {
  min-height: 100vh;
  background-color: #0f172a;
  color: #e2e8f0;
  display: flex;
  flex-direction: column;
}

.app-header {
  padding: 1rem 2rem;
  background: #1e293b;
  border-bottom: 1px solid #334155;
}

.app-header h1 {
  margin: 0;
  font-size: 1.5rem;
  color: #38bdf8;
}

.app-main {
  flex: 1;
  padding: 2rem;
  display: flex;
  flex-direction: column;
}
</style>
