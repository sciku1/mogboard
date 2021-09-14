class HighchartsFormatter
{
    formatExtendedHistory(extendedHistory)
    {
        // We could just take entries directly, but this gets us better typechecking
        const data = [...extendedHistory.entries];
        const hqData = data.filter(entry => entry.hq);
        const nqData = data.filter(entry => !entry.hq);
        return {
            series: [hqData, nqData],
        };
    }
}