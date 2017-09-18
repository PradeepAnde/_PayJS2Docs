const express = require('express');
const app = express();

const cors = require('cors')();
const bodyParser = require('body-parser');

const payjsAuth = require('@payjs/node')({
    merchantId: '999999999997',//417227771521',
    merchantKey: 'K3QD6YWYHFD',//I5T2R2K6V1Q3',
    clientId: 'GTq2h4mXxLIBtzbOWLO2GwqZfOgK8BbT',
    clientKey: 'ICkrA2n6HIleJ663',
});

app.use(cors);
app.use(bodyParser.json());

app.post('/auth', payjsAuth, (req, res) => {
    // the middleware blindly authenticates the request body!
    // make sure you're validating the actual content itself
    // (eg, you dont want to authorize a $1 payment when it should be $100)
    res.send(res.payjs);
});

app.listen(3001, () => {
    console.log('Sample PayJS+ExpressJS app listening on port 3001.')
});