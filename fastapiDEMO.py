# -*- coding: utf-8 -*-
"""
Created on Sun May  3 18:18:20 2026

@author: jtey4
"""

import uvicorn
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, field_validator

# --- 數據規格定義：判斷權移交至此 ---
class InferenceInput(BaseModel):
    sensor_id: str
    rpm: float

    @field_validator("sensor_id")
    @classmethod
    def check_id_format(cls, value):
        # 範例：判斷 ID 是否以 'CNC' 開頭 (這原本可能寫在 C#)
        if not value.startswith("CNC"):
            raise ValueError("無效的機台編號，必須以 CNC 開頭")
        return value

    @field_validator("rpm")
    @classmethod
    def check_rpm_limit(cls, value):
        if value < 0 or value > 10000:
            raise ValueError("轉速超出物理極限 (0-10000)")
        return value

api_server = FastAPI()

@api_server.post("/predict")
async def logic_judgment(payload: InferenceInput):
    # 這裡現在只跑純邏輯判斷，不跑模型
    # 模擬推理結果：根據 ID 和 RPM 給出狀態
    status = "⚠️ 異常" if payload.rpm > 5000 else "✅ 正常"
    
    return {
        "client_id": payload.sensor_id,
        "judgment": status,
        "message": f"機台 {payload.sensor_id} 當前轉速為 {payload.rpm}"
    }

if __name__ == "__main__":
    uvicorn.run(api_server, host="0.0.0.0", port=8000)