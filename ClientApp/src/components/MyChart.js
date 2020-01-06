import React from 'react'
import { Chart } from 'react-charts'

 export function MyChart() {
    const data = React.useMemo(
        () => [
            {
                label: 'Series 1',
                data: [[2000, 1], [2001, 2], [2002, 4], [2003, 2], [2004, 7]]
            },
            {
                label: 'Series 2',
                data: [[2000, 3], [2001, 1], [2002, 5], [2003, 6], [2004, 4]]
            }
        ],
        []
    )

    const axes = React.useMemo(
        () => [
            { primary: true, type: 'utc', position: 'bottom' },
            { type: 'linear', position: 'left' }
        ],
        []
    )


    const lineChart = (
        // A react-chart hyper-responsively and continuusly fills the available
        // space of its parent element automatically
        <div
            style={{
                width: '400px',
                height: '300px'
            }}
        >
            <Chart data={data} axes={axes} />
        </div>
     )
     return lineChart;
}