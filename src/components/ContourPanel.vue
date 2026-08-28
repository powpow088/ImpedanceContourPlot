<template>
  <div class="panel-card contour-panel">

    <div class="panel-content">
      <!-- 頂部佈局：左側控制區 + 右側資料匯入區 -->
      <div class="top-layout" style="display: flex; gap: 20px; align-items: stretch; margin-bottom: 20px;">

        <!-- 左側：資料匯入區塊 -->
        <div class="data-input-section" style="flex: 1; padding: 15px; background: rgba(0,0,0,0.2); border: 1px solid #4a5568; border-radius: 8px; display: flex; flex-direction: column;">
          <div style="display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 8px;">
            <div>
              <h3 style="margin-top: 0; color: #38bdf8; margin-bottom: 4px;">匯入資料 (CSV)</h3>
              <p style="font-size: 13px; color: #a0aec0; margin-bottom: 0;">請從 Excel 貼上或上傳資料：</p>
            </div>
            <div>
              <input type="file" accept=".csv,.txt,.tsv" @change="handleFileUpload" style="display: none;" ref="fileInput" />
              <button class="btn-secondary" @click="$refs.fileInput.click()">📁 上傳檔案</button>
            </div>
          </div>
          <textarea 
            v-model="pastedData" 
            rows="6" 
            placeholder="W&#9;H&#9;T&#9;Z0&#10;-10&#9;-10&#9;0&#9;48.5&#10;...請貼上資料..." 
            style="flex: 1; width: 100%; background: #1e293b; color: #fff; border: 1px solid #4a5568; padding: 10px; font-family: monospace; border-radius: 4px; resize: vertical;"
          ></textarea>
          
          <div style="margin-top: 10px; display: flex; gap: 10px; align-items: center;">
            <button class="btn-primary" @click="parseAndRender" style="width: 150px;">分析繪圖</button>
            <span v-if="parseError" style="color: #ff4757; font-weight: bold;">{{ parseError }}</span>
            <span v-if="parseSuccess" style="color: #00f2fe; font-weight: bold;">{{ parseSuccess }}</span>
          </div>
        </div>
        
        <!-- <div class="control-section"> -->
          <!-- 右側：控制與結果區 -->
          <div class="control-section" style="flex: 1; display: flex; flex-direction: column; justify-content: space-between;">
            <div v-if="!contourData" style="color: #a0aec0; padding: 20px; text-align: center; background: rgba(0,0,0,0.2); border-radius: 8px; border: 1px dashed #4a5568; flex: 1; display: flex; align-items: center; justify-content: center;">
               請先匯入資料
            </div>
            
            <div class="results-container" v-show="contourData" style="flex: 1; display: flex; flex-direction: column;">
              <!-- 資料彙整顯示區 -->
              <div class="summary-info" v-if="summaryData" style="flex: 1; box-sizing: border-box; background: rgba(0,0,0,0.2); padding: 15px; border-radius: 8px; border: 1px solid #4a5568;">
                
                <h4 style="margin-top: 0; color: #4ade80; margin-bottom: 4px;">動態調整區</h4>
                <div class="summary-item" style="display: flex; align-items: center; gap: 8px;">
                  <span class="check-icon">🎯</span> 阻抗管控目標: &plusmn; 
                  <input type="number" v-model.number="z0Target" @change="drawAllPlots" min="0" step="0.5" style="width: 60px; background: rgba(255,255,255,0.1); border: 1px solid #4a5568; color: #fff; padding: 2px 4px; border-radius: 4px; text-align: center;"> %
                </div>
                <div class="summary-item" style="display: flex; align-items: center; gap: 8px;">
                  <span class="check-icon">🚧</span> 線寬管控目標 (黃線): &plusmn; 
                  <input type="number" v-model.number="wLimitTarget" @change="drawAllPlots" min="0" step="0.5" style="width: 60px; background: rgba(255,255,255,0.1); border: 1px solid #ffcc00; color: #fff; padding: 2px 4px; border-radius: 4px; text-align: center;"> %
                </div>
                <div class="summary-item" style="display: flex; align-items: center; gap: 8px;">
                  <span class="check-icon">🚧</span> 銅厚管控目標 (篩選極值): &plusmn; 
                  <input type="number" v-model.number="tLimitTarget" @change="drawAllPlots" min="0" :step="tStep" style="width: 60px; background: rgba(255,255,255,0.1); border: 1px solid #4ade80; color: #fff; padding: 2px 4px; border-radius: 4px; text-align: center;"> %
                </div>

                
                <div style="margin-top: 10px; padding-top: 10px; border-top: 1px solid #334155;">
                  <div class="summary-item" :style="wLimitTarget > summaryData.wLimit ? 'color: #ff4757; font-weight: bold;' : ''">
                    <span class="check-icon">{{ wLimitTarget > summaryData.wLimit ? '❌' : '✔️' }}</span> 
                    線寬資料範圍: &plusmn; {{ summaryData.wLimit }}%
                    <span v-if="wLimitTarget > summaryData.wLimit" style="color: #ff4757; font-weight: bold; margin-left: 8px;">(超出範圍)</span>
                  </div>
                  <div class="summary-item" :style="tLimitTarget > summaryData.tLimit ? 'color: #ff4757; font-weight: bold;' : ''">
                    <span class="check-icon">{{ tLimitTarget > summaryData.tLimit ? '❌' : '✔️' }}</span> 
                    銅厚資料範圍: &plusmn; {{ summaryData.tLimit }}%
                    <span v-if="tLimitTarget > summaryData.tLimit" style="color: #ff4757; font-weight: bold; margin-left: 8px;">(超出範圍)</span>
                  </div>
                  <div class="summary-item" v-if="summaryData.hasIntersection">
                    <span class="check-icon">✔️</span> (H) 全域安全交集: <span style="color: #ff69b4; font-weight: bold; margin: 0 8px;">{{ summaryData.hLimitLeft }}% ~ {{ summaryData.hLimitRight > 0 ? '+' : '' }}{{ summaryData.hLimitRight }}%</span>
                  </div>
                  <div class="summary-item" v-else>
                    <span class="check-icon">❌</span> (H) 全域安全交集: <span style="color: #ff4757; font-weight: bold; margin: 0 8px;">無交集 (條件過嚴)</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

        <!-- </div> -->

      </div>

      <!-- 底部：繪圖區 -->
      <div class="results-container" v-show="contourData">
        <!-- 繪圖區選項 -->
        <div class="toolbar" style="display: flex; justify-content: flex-end; margin-bottom: 15px;">
          <div style="margin-right: 15px; display: flex; gap: 10px; color: #a0aec0; align-items: center; background: rgba(0,0,0,0.2); padding: 4px 10px; border-radius: 4px;">
            <span>顯示模式:</span>
            <label style="cursor: pointer;"><input type="radio" v-model="displayMode" value="extremes" @change="requestPlotRedraw" /> T 極值 (在管控目標內)</label>
            <label style="cursor: pointer;"><input type="radio" v-model="displayMode" value="all" @change="requestPlotRedraw" /> 全部</label>
          </div>
          <label class="checkbox-label" title="在圖上顯示實際座標點" style="display: flex; align-items: center;">
            <input type="checkbox" v-model="showSamplingPoints" @change="drawAllPlots" style="margin-right: 5px;" />
            顯示點位
          </label>
        </div>

        <!-- 繪圖區 -->
        <div class="charts-grid">
          <div class="chart-wrapper" v-for="matrix in sortedMatrices" :key="matrix.t_pct">
            <div :id="`plotly-contour-t${matrix.t_pct}`" class="chart-container"></div>
            <div class="h-range-label" v-if="localHRanges[`plotly-contour-t${matrix.t_pct}`]">
              H 範圍: <span class="h-val">{{ localHRanges[`plotly-contour-t${matrix.t_pct}`].left }}% ~ {{ localHRanges[`plotly-contour-t${matrix.t_pct}`].right > 0 ? '+' : '' }}{{ localHRanges[`plotly-contour-t${matrix.t_pct}`].right }}%</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, nextTick } from 'vue'
import Papa from 'papaparse'

const smoothInterpolation = ref(true)
const showSamplingPoints = ref(false)
const contourData = ref(null)
const summaryData = ref(null)
const z0Target = ref(5.0) 
const wLimitTarget = ref(5.0) 
const tLimitTarget = ref(10.0) 
const tStep = ref(1.0)

const localHRanges = ref({})

const pastedData = ref('')
const parseError = ref('')
const parseSuccess = ref('')

const displayMode = ref('extremes')

const sortedMatrices = computed(() => {
  if (!contourData.value || !contourData.value.matrices) return []
  const filteredMatrices = contourData.value.matrices.filter(m => Math.abs(m.t_pct) <= tLimitTarget.value);
  let all = [...filteredMatrices].sort((a, b) => a.t_pct - b.t_pct);
  
  if (displayMode.value === 'extremes' && all.length > 3) {
    const min = all[0];
    const max = all[all.length - 1];
    const zero = all.find(m => m.t_pct === 0) || all[Math.floor(all.length / 2)];
    all = Array.from(new Set([min, zero, max])).sort((a, b) => a.t_pct - b.t_pct);
  }
  
  return all.sort((a, b) => {
    if (a.t_pct === 0) return -1;
    if (b.t_pct === 0) return 1;
    return a.t_pct - b.t_pct;
  });
})

function requestPlotRedraw() {
  nextTick(() => {
    drawAllPlots();
  });
}

const fileInput = ref(null)

function handleFileUpload(event) {
  const file = event.target.files[0]
  if (!file) return
  
  const reader = new FileReader()
  reader.onload = (e) => {
    // 移除 Excel 導出 CSV 時，行尾常見的多餘逗號 (例如: ,,,,,)
    let cleanedText = e.target.result.replace(/,+(\r?\n|\r|$)/g, '$1')
    pastedData.value = cleanedText
    parseAndRender()
  }
  reader.readAsText(file)
  
  event.target.value = ''
}

function parseAndRender() {
  parseError.value = ''
  parseSuccess.value = ''
  contourData.value = null
  
  if (!pastedData.value.trim()) {
    parseError.value = '請先貼上資料！'
    return
  }

  Papa.parse(pastedData.value.trim(), {
    header: true,
    dynamicTyping: true,
    skipEmptyLines: true,
    complete: function(results) {
      if (results.errors.length > 0 && !results.data.length) {
        parseError.value = '資料解析失敗，請確認格式是否正確。'
        return
      }

      const data = results.data;
      if (data.length === 0) {
        parseError.value = '無有效資料列。'
        return
      }

      // 檢查必要欄位 (W, H, T, Z0 或 Z)
      const keys = Object.keys(data[0]).map(k => k.trim().toUpperCase());
      const hasW = keys.includes('W') || keys.includes('W_PCT');
      const hasH = keys.includes('H') || keys.includes('H_PCT');
      const hasT = keys.includes('T') || keys.includes('T_PCT');
      const hasZ = keys.includes('Z0') || keys.includes('Z') || keys.includes('Z0_PCT');

      if (!hasW || !hasH || !hasT || !hasZ) {
        parseError.value = '找不到必要的欄位 (需要包含 W, H, T, Z0)。請確認表頭名稱。';
        return;
      }

      // 找出實際的 key 名稱
      const k_w = Object.keys(data[0]).find(k => k.trim().toUpperCase() === 'W' || k.trim().toUpperCase() === 'W_PCT');
      const k_h = Object.keys(data[0]).find(k => k.trim().toUpperCase() === 'H' || k.trim().toUpperCase() === 'H_PCT');
      const k_t = Object.keys(data[0]).find(k => k.trim().toUpperCase() === 'T' || k.trim().toUpperCase() === 'T_PCT');
      const k_z = Object.keys(data[0]).find(k => k.trim().toUpperCase() === 'Z0' || k.trim().toUpperCase() === 'Z' || k.trim().toUpperCase() === 'Z0_PCT');

      // 提取唯一的 W, H, T 值並排序
      const wSet = new Set();
      const hSet = new Set();
      const tSet = new Set();

            const parseNum = (val) => {
        if (typeof val === 'number') return val;
        if (typeof val === 'string') {
          const num = parseFloat(val.replace('%', '').trim());
          return isNaN(num) ? null : num;
        }
        return null;
      };

      for (let i = 0; i < data.length; i++) {
        const row = data[i];
        let w = parseNum(row[k_w]); 
        let h = parseNum(row[k_h]); 
        let t = parseNum(row[k_t]); 
        let z = parseNum(row[k_z]);
        
        if (w === null || h === null || t === null || z === null) {
          parseError.value = '第 ' + (i+1) + ' 行資料型態錯誤，包含無法解析的非數值。';
          return;
        }
        
        row[k_w] = w;
        row[k_h] = h;
        row[k_t] = t;
        row[k_z] = z;

        wSet.add(w);
        hSet.add(h);
        tSet.add(t);
      }

      const wVals = Array.from(wSet).sort((a,b)=>a-b);
      const hVals = Array.from(hSet).sort((a,b)=>a-b);
      const tVals = Array.from(tSet).sort((a,b)=>a-b);

      const matrices = [];

      for (const t of tVals) {
        const zMatrix = [];
        for (let i = 0; i < wVals.length; i++) {
          const rowArr = [];
          for (let j = 0; j < hVals.length; j++) {
            const point = data.find(d => d[k_w] === wVals[i] && d[k_h] === hVals[j] && d[k_t] === t);
            rowArr.push(point ? point[k_z] : null); // 如果網格缺值補 null
          }
          zMatrix.push(rowArr);
        }
        matrices.push({
          t_pct: t,
          z_matrix: zMatrix
        });
      }

      contourData.value = {
        w_axis_pct: wVals,
        h_axis_pct: hVals,
        matrices: matrices
      };

      const maxT = Math.max(...matrices.map(m => Math.abs(m.t_pct))) || 10;
      tLimitTarget.value = maxT; // 自動將 T 管控目標設為資料的最大變異值
      
      // 自動計算 T 的間距 (step) 以符合資料特性
      const positiveTVals = Array.from(new Set(matrices.map(m => Math.abs(m.t_pct)))).sort((a, b) => a - b);
      if (positiveTVals.length > 1) {
        // 取最小兩個非負值的差當作間距 (例如: [0, 5, 10] -> 5 - 0 = 5)
        tStep.value = positiveTVals[1] - positiveTVals[0];
      } else {
        tStep.value = 1.0;
      }

      parseSuccess.value = `解析成功！共 ${data.length} 筆資料 (${wVals.length}x${hVals.length})`;
      
      nextTick(() => {
        drawAllPlots();
      });
    }
  });
}

function drawAllPlots() {
  if (!contourData.value) return
  
  if (z0Target.value !== null && z0Target.value !== undefined) z0Target.value = Math.abs(z0Target.value)
  if (wLimitTarget.value !== null && wLimitTarget.value !== undefined) wLimitTarget.value = Math.abs(wLimitTarget.value)
  if (tLimitTarget.value !== null && tLimitTarget.value !== undefined) tLimitTarget.value = Math.abs(tLimitTarget.value)

  const xVals = contourData.value.h_axis_pct 
  const yVals = contourData.value.w_axis_pct 
  
  const z0Thresh = z0Target.value || 5.0
  const limitW = wLimitTarget.value || 5.0   
  
  function getZ(w, h, zMatrix) {
    const findInterval = (arr, val) => {
      if (val <= arr[0]) return [0, 0, 0];
      if (val >= arr[arr.length - 1]) return [arr.length - 1, arr.length - 1, 0];
      for (let i = 0; i < arr.length - 1; i++) {
        if (val >= arr[i] && val <= arr[i + 1]) {
          const ratio = (val - arr[i]) / (arr[i + 1] - arr[i]);
          return [i, i + 1, ratio];
        }
      }
      return [0, 0, 0];
    };
    
    const [w1, w2, rw] = findInterval(yVals, w);
    const [h1, h2, rh] = findInterval(xVals, h);
    
    const z11 = zMatrix[w1][h1] || 0;
    const z12 = zMatrix[w1][h2] || 0;
    const z21 = zMatrix[w2][h1] || 0;
    const z22 = zMatrix[w2][h2] || 0;
    
    const zBottom = z11 + rh * (z12 - z11);
    const zTop = z21 + rh * (z22 - z21);
    return zBottom + rw * (zTop - zBottom);
  }

  function findHIntersection(wVal, targetZ0, zMatrix, isLeftBound) {
    const sampleH = [];
    for(let h = -10; h <= 10; h += 0.5) sampleH.push(h);
    
    for (let j = 0; j < sampleH.length - 1; j++) {
      const z1 = getZ(wVal, sampleH[j], zMatrix);
      const z2 = getZ(wVal, sampleH[j+1], zMatrix);
      
      if ((z1 <= targetZ0 && z2 >= targetZ0) || (z1 >= targetZ0 && z2 <= targetZ0)) {
        if (z2 === z1) return sampleH[j];
        const ratio = (targetZ0 - z1) / (z2 - z1);
        return sampleH[j] + ratio * (sampleH[j+1] - sampleH[j]);
      }
    }
    const zMin = getZ(wVal, sampleH[0], zMatrix);
    const zMax = getZ(wVal, sampleH[sampleH.length-1], zMatrix);
    const extLimit = 15; 
    
    if (isLeftBound) {
      if (zMax < targetZ0) return extLimit;
      if (zMin > targetZ0) return -extLimit;
    } else {
      if (zMin > targetZ0) return -extLimit;
      if (zMax < targetZ0) return extLimit;
    }
    return isLeftBound ? -extLimit : extLimit;
  }

  try {
    let globalHLeft = -15;
    let globalHRight = 15;
    const graphData = [];
    const displayTSet = new Set(sortedMatrices.value.map(m => m.t_pct));
    
    // 只計算在 tLimitTarget 範圍內的 T，來決定全域安全交集
    const validMatrices = contourData.value.matrices.filter(m => Math.abs(m.t_pct) <= tLimitTarget.value);
    
    for (const matrixObj of validMatrices) {
      const tVal = matrixObj.t_pct;
      const zMatrix = matrixObj.z_matrix;
      
      const localHLeft = findHIntersection(limitW, -z0Thresh, zMatrix, true);
      const localHRight = findHIntersection(-limitW, z0Thresh, zMatrix, false);
      
      globalHLeft = Math.max(globalHLeft, localHLeft);
      globalHRight = Math.min(globalHRight, localHRight);
      
      if (displayTSet.has(tVal)) {
        const divId = `plotly-contour-t${tVal}`;
        
        localHRanges.value[divId] = {
          left: localHLeft.toFixed(1),
          right: localHRight.toFixed(1)
        }
        
        graphData.push({ tVal, zMatrix, localHLeft, localHRight, divId });
      }
    }

    summaryData.value = {
      wLimit: Math.max(...yVals.map(Math.abs)) || 10,
      tLimit: Math.max(...contourData.value.matrices.map(m => Math.abs(m.t_pct))) || 10,
      hLimitLeft: globalHLeft.toFixed(1),
      hLimitRight: globalHRight.toFixed(1),
      z0Thresh: z0Thresh,
      hasIntersection: globalHLeft <= globalHRight
    }
    
    const maxRange = 12; 
    const span = maxRange * 2;
    const toRatio = (val) => Math.max(0, Math.min(1, (val - (-maxRange)) / span));
    
    const tB = 10;
    const tC = 6;
    const tW = 2; 
    
    const colorscale = [
      [0, '#004c99'], [toRatio(-tB), '#004c99'],
      [toRatio(-tB), '#66b2ff'], [toRatio(-tC), '#66b2ff'],
      [toRatio(-tC), '#b2ebf2'], [toRatio(-tW), '#b2ebf2'],
      [toRatio(-tW), '#ffffff'], [toRatio(tW), '#ffffff'], 
      [toRatio(tW), '#dcedc8'], [toRatio(tC), '#dcedc8'],
      [toRatio(tC), '#66bb6a'], [toRatio(tB), '#66bb6a'],
      [toRatio(tB), '#1b5e20'], [1, '#1b5e20']
    ];

    for (const gd of graphData) {
      const trace = {
        z: gd.zMatrix,
        x: xVals,
        y: yVals,
        type: 'contour',
        colorscale: colorscale,
        contours: {
          start: -maxRange,
          end: maxRange,
          size: 1, 
          showlines: true,
          line: { color: 'rgba(0,0,0,0.2)', width: 0.5 }
        },
        line: { smoothing: smoothInterpolation.value ? 1.3 : 0 },
        colorbar: {
          title: 'Z0 (%)',
          titleside: 'right',
          titlefont: { color: '#a0aec0' },
          tickfont: { color: '#a0aec0' },
          thickness: 10
        }
      };

      const traces = [trace];

      if (showSamplingPoints.value) {
        const scatterX = [];
        const scatterY = [];
        for (let i = 0; i < xVals.length; i++) {
          for (let j = 0; j < yVals.length; j++) {
            scatterX.push(xVals[i]);
            scatterY.push(yVals[j]);
          }
        }
        traces.push({
          x: scatterX,
          y: scatterY,
          mode: 'markers',
          type: 'scatter',
          marker: {
            color: 'rgba(0, 0, 0, 0.4)',
            size: 5,
            line: { color: 'rgba(0, 0, 0, 0.9)', width: 1 }
          },
          hoverinfo: 'none',
          showlegend: false
        });
      }

      const shapes = [
        { type: 'line', x0: -15, x1: 15, y0: limitW, y1: limitW, line: { color: '#fbbf24', width: 2 } },
        { type: 'line', x0: -15, x1: 15, y0: -limitW, y1: -limitW, line: { color: '#fbbf24', width: 2 } },
        { type: 'line', y0: -15, y1: 15, x0: gd.localHLeft, x1: gd.localHLeft, line: { color: '#3b82f6', width: 2, dash: 'dash' } },
        { type: 'line', y0: -15, y1: 15, x0: gd.localHRight, x1: gd.localHRight, line: { color: '#3b82f6', width: 2, dash: 'dash' } }
      ];

      if (globalHLeft <= globalHRight) {
        shapes.unshift({ type: 'rect', x0: globalHLeft, x1: globalHRight, y0: -limitW, y1: limitW, fillcolor: '#ff69b4', opacity: 0.25, line: { width: 0 } });
      }

      const layout = {
        title: { text: `T = ${gd.tVal}%`, font: { color: '#e2e8f0', family: 'Inter', size: 14 } },
        paper_bgcolor: 'transparent',
        plot_bgcolor: 'transparent',
        xaxis: { title: 'H (%)', range: [-12, 12], color: '#a0aec0', gridcolor: '#2d3748', zerolinecolor: '#4a5568' },
        yaxis: { title: 'W (%)', range: [-12, 12], color: '#a0aec0', gridcolor: '#2d3748', zerolinecolor: '#4a5568' },
        shapes: shapes,
        margin: { l: 40, r: 10, t: 30, b: 40 }
      };

      if (window.Plotly) {
        setTimeout(() => {
          try {
            window.Plotly.newPlot(gd.divId, traces, layout, { responsive: true, displayModeBar: false });
          } catch (err) {
            console.error(`Plotly Error on ${gd.divId}:`, err);
          }
        }, 100);
      }
    }
  } catch (globalErr) {
    console.error("drawAllPlots error:", globalErr);
  }
}
</script>

<style scoped>
.contour-panel {
  margin-top: 16px;
  position: relative;
}
.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid #334155;
  padding-bottom: 12px;
  margin-bottom: 20px;
}
.panel-title {
  font-size: 1.25rem;
  color: #38bdf8;
  margin: 0;
}
.toolbar {
  display: flex;
  gap: 12px;
  align-items: center;
}
.btn-primary {
  background: #3b82f6;
  color: #fff;
  border: none;
  padding: 6px 12px;
  border-radius: 4px;
  cursor: pointer;
  font-weight: bold;
}
.btn-primary:hover {
  background: #2563eb;
}
.btn-secondary {
  background: #475569;
  color: #f8fafc;
  border: 1px solid #64748b;
  padding: 6px 12px;
  border-radius: 4px;
  cursor: pointer;
  font-weight: bold;
  transition: all 0.2s;
}
.btn-secondary:hover {
  background: #334155;
  border-color: #94a3b8;
}
.charts-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 20px;
}
.chart-container {
  height: 350px;
  background: rgba(15, 23, 42, 0.5);
  border: 1px solid #334155;
  border-radius: 8px;
}
.h-range-label {
  text-align: center;
  margin-top: 8px;
  font-size: 13px;
  color: #a0aec0;
}
.h-val {
  color: #3b82f6;
  font-weight: bold;
}
.summary-item {
  color: #e2e8f0;
  margin-bottom: 6px;
}
</style>

