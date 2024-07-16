import React, { useEffect, useState, useContext } from 'react';
import { createUseStyles } from 'react-jss';
import { PieChart, Pie, Cell } from 'recharts';
import { useTranslation } from 'react-i18next';
import { getStatistics } from '../../services/statisticsService';
import { StatusContext } from '../../App';
import { apiDirectDecorator, apiDecoratorOptions } from '../../helpers/api'

const styles = createUseStyles({
    wrapper: {
        dispay: "flex"
    }
})

const renderLabel = (entry) => { return entry.name; }

export default function Statistics() {

    const classes = styles();
    const { t } = useTranslation();
    const statusContext = useContext(StatusContext);
    const [appRejectionStateCounts, setAppRejectionStateCounts] = useState([]);

    useEffect(() => {
        apiDirectDecorator(
            async () => await getStatistics(),
            apiDecoratorOptions(
                statusContext,
                (data) => setAppRejectionStateCounts(data?.applicationRejectionStateCounts ?? []),
                () => {}))
            ();
    },[]);

    return (
        <div className={classes.wrapper}>
            <div>
                <fieldset>
                    <legend>{t("statistics.rejection-state-counts")}</legend>
                        <PieChart width={500} height={400}>
                            <Pie label={renderLabel}
                                dataKey="value"
                                startAngle={0}
                                endAngle={360}
                                data={appRejectionStateCounts}
                                cx="50%"
                                cy="50%"
                                name={t("statistics.rejection-state-counts")}
                                outerRadius={80}>

                                {appRejectionStateCounts.map((entry, index) => (
                                    <Cell key={`cell-${index}`} fill={`#${entry.color}`} />)
                                )}
                            </Pie>
                        </PieChart>
                </fieldset>
            </div>
        </div>
    )
}