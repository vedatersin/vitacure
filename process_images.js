const Jimp = require('jimp');
const fs = require('fs');
const path = require('path');

const imgDir = path.join(__dirname, 'wwwroot', 'img');

async function processImage(filename) {
    const fullPath = path.join(imgDir, filename);
    if (!fs.existsSync(fullPath)) {
        console.error('File not found:', fullPath);
        return;
    }

    console.log(`Processing ${filename}...`);
    try {
        const image = await Jimp.read(fullPath);

        // Find background color (top-left pixel is assumed to be the solid green screen)
        const bgColor = Jimp.intToRGBA(image.getPixelColor(10, 10));

        const tolerance = 70; // 0-255 tolerance for antialiased edges

        // Set alpha to 0 for matching pixels
        image.scan(0, 0, image.bitmap.width, image.bitmap.height, function (x, y, idx) {
            const r = this.bitmap.data[idx];
            const g = this.bitmap.data[idx + 1];
            const b = this.bitmap.data[idx + 2];

            const distance = Math.sqrt(
                Math.pow(r - bgColor.r, 2) +
                Math.pow(g - bgColor.g, 2) +
                Math.pow(b - bgColor.b, 2)
            );

            // If it is very close to the background color (green screen)
            if (distance < tolerance) {
                this.bitmap.data[idx + 3] = 0; // Transparent
            }
        });

        const outputPath = path.join(imgDir, filename.replace('_green.png', '_nobg.png'));

        // Handle callback for older jimp versions
        return new Promise((resolve, reject) => {
            image.write(outputPath, (err) => {
                if (err) return reject(err);
                console.log(`Done processing ${filename}! Output at ${outputPath}`);
                resolve();
            });
        });
    } catch (err) {
        console.error(`Error processing ${filename}:`, err);
        fs.writeFileSync('error_jimp.txt', err.stack || err.toString());
    }
}

async function main() {
    await processImage('bottle_multi_green.png');
    await processImage('bottle_d3_green.png');
    await processImage('bottle_mag_green.png');
    await processImage('bottle_omega_green.png');
}

main();
