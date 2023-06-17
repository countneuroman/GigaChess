<script setup lang="ts">
import { ref, onMounted } from "vue";
import { Chessground } from 'chessground/chessground';
import axios from 'axios';
import '@/assets/board.css';
const boardDiv = ref<HTMLElement | null>(null);
const axiosInstance = axios.create({
  baseURL: 'http://localhost:5000/'
});

function fetchData() {
  const url = 'Chess';
  const data = { key: 'value' };

  axiosInstance.post(url)
  .then(response => {
    console.log(response.data);
  })
  .catch(error => {
    console.error(error);
  });

}

onMounted(() => {
  const config = {};
  if (boardDiv.value) {
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
  <button v-on:click="fetchData()">Fetch data</button>
</template>

<style>
body {
  background-color: #222;
}
</style>