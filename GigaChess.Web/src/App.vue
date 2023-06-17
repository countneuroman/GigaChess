<script setup lang="ts">
import { ref, onMounted } from "vue";
import { Chessground } from 'chessground/chessground';
import '@/assets/board.css';
const boardDiv = ref<HTMLElement | null>(null);

onMounted(() => {
    const config = {};
    if(boardDiv.value) {
      const ground = Chessground(boardDiv.value, config);
    }
  });
</script>

<template>
  <div class="main-wrap">
    <div class="main-board">
      <div ref="boardDiv"></div>
    </div>
  </div>
</template>

<style>
body {
  background-color: #222;
}
</style>